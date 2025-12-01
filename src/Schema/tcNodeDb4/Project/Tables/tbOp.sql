CREATE TABLE [Project].[tbOp] (
    [ProjectCode]        NVARCHAR (20)   NOT NULL,
    [OperationNumber] SMALLINT        NOT NULL,
    [SyncTypeCode]    SMALLINT        CONSTRAINT [DF_Project_tbOp_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OpStatusCode]    SMALLINT        CONSTRAINT [DF_Project_tbOp_OpStatusCode] DEFAULT ((0)) NOT NULL,
    [UserId]          NVARCHAR (10)   NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [Note]            NVARCHAR(MAX)           NULL,
    [StartOn]         DATETIME        CONSTRAINT [DF_Project_tbOp_StartOn] DEFAULT (getdate()) NOT NULL,
    [EndOn]           DATETIME        CONSTRAINT [DF_Project_tbOp_EndOn] DEFAULT (getdate()) NOT NULL,
    [OffsetDays]      SMALLINT        CONSTRAINT [DF_Project_tbOp_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Project_tbOp_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Project_tbOp_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Project_tbOp_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Project_tbOp_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) CONSTRAINT [DF_Project_tbOp_Duration] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Project_tbOp] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Project_tbOp_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Project_tbOp_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tbOp_Project_tbOpStatus] FOREIGN KEY ([OpStatusCode]) REFERENCES [Project].[tbOpStatus] ([OpStatusCode]),
    CONSTRAINT [FK_Project_tbOp_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbOp_OpStatusCode]
    ON [Project].[tbOp]([OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbOp_UserIdOpStatus]
    ON [Project].[tbOp]([UserId] ASC, [OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Project.Project_tbOp_TriggerUpdate 
   ON  Project.tbOp 
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @Msg NVARCHAR(MAX);

		UPDATE ops
		SET StartOn = CAST(ops.StartOn AS DATE), EndOn = CAST(ops.EndOn AS DATE)
		FROM Project.tbOp ops JOIN inserted i ON ops.ProjectCode = i.ProjectCode AND ops.OperationNumber = i.OperationNumber
		WHERE (DATEDIFF(SECOND, CAST(i.StartOn AS DATE), i.StartOn) <> 0 
				OR DATEDIFF(SECOND, CAST(i.EndOn AS DATE), i.EndOn) <> 0);
					
		IF EXISTS (	SELECT *
				FROM inserted
					JOIN Project.tbOp ops ON inserted.ProjectCode = ops.ProjectCode AND inserted.OperationNumber = ops.OperationNumber
				WHERE inserted.StartOn > inserted.EndOn)
			BEGIN
			UPDATE ops
			SET EndOn = ops.StartOn
			FROM Project.tbOp ops JOIN inserted i ON ops.ProjectCode = i.ProjectCode AND ops.OperationNumber = i.OperationNumber;
						
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3016;
			EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 1		
			END;

		WITH Projects AS
		(
			SELECT ProjectCode FROM inserted GROUP BY ProjectCode
		), last_calloff AS
		(
			SELECT ops.ProjectCode, MAX(OperationNumber) AS OperationNumber
			FROM Project.tbOp ops JOIN Projects ON ops.ProjectCode = Projects.ProjectCode	
			WHERE SyncTypeCode = 2 
			GROUP BY ops.ProjectCode
		), calloff AS
		(
			SELECT inserted.ProjectCode, inserted.EndOn FROM inserted 
			JOIN last_calloff ON inserted.ProjectCode = last_calloff.ProjectCode AND inserted.OperationNumber = last_calloff.OperationNumber
			WHERE SyncTypeCode = 2
		)
		UPDATE Project
		SET ActionOn = calloff.EndOn
		FROM Project.tbProject Project
		JOIN calloff ON Project.ProjectCode = calloff.ProjectCode
		WHERE calloff.EndOn <> Project.ActionOn AND Project.ProjectStatusCode < 3;

		UPDATE Project.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbOp INNER JOIN inserted AS i ON tbOp.ProjectCode = i.ProjectCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
