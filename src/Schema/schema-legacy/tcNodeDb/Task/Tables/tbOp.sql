CREATE TABLE [Task].[tbOp] (
    [TaskCode]        NVARCHAR (20)   NOT NULL,
    [OperationNumber] SMALLINT        NOT NULL,
    [SyncTypeCode]    SMALLINT        CONSTRAINT [DF_Task_tbOp_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OpStatusCode]    SMALLINT        CONSTRAINT [DF_Task_tbOp_OpStatusCode] DEFAULT ((0)) NOT NULL,
    [UserId]          NVARCHAR (10)   NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [Note]            NTEXT           NULL,
    [StartOn]         DATETIME        CONSTRAINT [DF_Task_tbOp_StartOn] DEFAULT (getdate()) NOT NULL,
    [EndOn]           DATETIME        CONSTRAINT [DF_Task_tbOp_EndOn] DEFAULT (getdate()) NOT NULL,
    [OffsetDays]      SMALLINT        CONSTRAINT [DF_Task_tbOp_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Task_tbOp_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Task_tbOp_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Task_tbOp_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Task_tbOp_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) CONSTRAINT [DF_Task_tbOp_Duration] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Task_tbOp] PRIMARY KEY CLUSTERED ([TaskCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Task_tbOp_Activity_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Activity].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Task_tbOp_Task_tb] FOREIGN KEY ([TaskCode]) REFERENCES [Task].[tbTask] ([TaskCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Task_tbOp_Task_tbOpStatus] FOREIGN KEY ([OpStatusCode]) REFERENCES [Task].[tbOpStatus] ([OpStatusCode]),
    CONSTRAINT [FK_Task_tbOp_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbOp_OpStatusCode]
    ON [Task].[tbOp]([OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbOp_UserIdOpStatus]
    ON [Task].[tbOp]([UserId] ASC, [OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Task.Task_tbOp_TriggerUpdate 
   ON  Task.tbOp 
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @Msg NVARCHAR(MAX);

		UPDATE ops
		SET StartOn = CAST(ops.StartOn AS DATE), EndOn = CAST(ops.EndOn AS DATE)
		FROM Task.tbOp ops JOIN inserted i ON ops.TaskCode = i.TaskCode AND ops.OperationNumber = i.OperationNumber
		WHERE (DATEDIFF(SECOND, CAST(i.StartOn AS DATE), i.StartOn) <> 0 
				OR DATEDIFF(SECOND, CAST(i.EndOn AS DATE), i.EndOn) <> 0);
					
		IF EXISTS (	SELECT *
				FROM inserted
					JOIN Task.tbOp ops ON inserted.TaskCode = ops.TaskCode AND inserted.OperationNumber = ops.OperationNumber
				WHERE inserted.StartOn > inserted.EndOn)
			BEGIN
			UPDATE ops
			SET EndOn = ops.StartOn
			FROM Task.tbOp ops JOIN inserted i ON ops.TaskCode = i.TaskCode AND ops.OperationNumber = i.OperationNumber;
						
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3016;
			EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 1		
			END;

		WITH tasks AS
		(
			SELECT TaskCode FROM inserted GROUP BY TaskCode
		), last_calloff AS
		(
			SELECT ops.TaskCode, MAX(OperationNumber) AS OperationNumber
			FROM Task.tbOp ops JOIN tasks ON ops.TaskCode = tasks.TaskCode	
			WHERE SyncTypeCode = 2 
			GROUP BY ops.TaskCode
		), calloff AS
		(
			SELECT inserted.TaskCode, inserted.EndOn FROM inserted 
			JOIN last_calloff ON inserted.TaskCode = last_calloff.TaskCode AND inserted.OperationNumber = last_calloff.OperationNumber
			WHERE SyncTypeCode = 2
		)
		UPDATE task
		SET ActionOn = calloff.EndOn
		FROM Task.tbTask task
		JOIN calloff ON task.TaskCode = calloff.TaskCode
		WHERE calloff.EndOn <> task.ActionOn AND task.TaskStatusCode < 3;

		UPDATE Task.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbOp INNER JOIN inserted AS i ON tbOp.TaskCode = i.TaskCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
