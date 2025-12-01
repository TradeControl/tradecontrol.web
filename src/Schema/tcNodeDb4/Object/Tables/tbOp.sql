CREATE TABLE [Object].[tbOp] (
    [ObjectCode]    NVARCHAR (50)   NOT NULL,
    [OperationNumber] SMALLINT        CONSTRAINT [DF_Object_tbOp_OperationNumber] DEFAULT ((0)) NOT NULL,
    [SyncTypeCode]    SMALLINT        CONSTRAINT [DF_Object_tbOp_SyncTypeCode] DEFAULT ((1)) NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [OffsetDays]      SMALLINT        CONSTRAINT [DF_Object_tbOp_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Object_tbOp_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Object_tbOp_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Object_tbOp_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Object_tbOp_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) CONSTRAINT [DF_Object_tbOp_Duration] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Object_tbOp] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Object_tbOp_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Object_tbOp_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbOp_Operation]
    ON [Object].[tbOp]([Operation] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Object.Object_tbOp_TriggerUpdate 
   ON  Object.tbOp 
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Object.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbOp INNER JOIN inserted AS i ON tbOp.ObjectCode = i.ObjectCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
