CREATE TABLE [Activity].[tbOp] (
    [ActivityCode]    NVARCHAR (50)   NOT NULL,
    [OperationNumber] SMALLINT        CONSTRAINT [DF_Activity_tbOp_OperationNumber] DEFAULT ((0)) NOT NULL,
    [SyncTypeCode]    SMALLINT        CONSTRAINT [DF_Activity_tbOp_SyncTypeCode] DEFAULT ((1)) NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [OffsetDays]      SMALLINT        CONSTRAINT [DF_Activity_tbOp_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Activity_tbOp_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Activity_tbOp_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Activity_tbOp_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Activity_tbOp_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) CONSTRAINT [DF_Activity_tbOp_Duration] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Activity_tbOp] PRIMARY KEY CLUSTERED ([ActivityCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Activity_tbOp_Activity_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Activity].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Activity_tbOp_tbActivity] FOREIGN KEY ([ActivityCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbOp_Operation]
    ON [Activity].[tbOp]([Operation] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Activity.Activity_tbOp_TriggerUpdate 
   ON  Activity.tbOp 
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Activity.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Activity.tbOp INNER JOIN inserted AS i ON tbOp.ActivityCode = i.ActivityCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
