CREATE TABLE [Object].[tbFlow] (
    [ParentCode]     NVARCHAR (50)   NOT NULL,
    [StepNumber]     SMALLINT        CONSTRAINT [DF_Object_tbFlow_StepNumber] DEFAULT ((10)) NOT NULL,
    [ChildCode]      NVARCHAR (50)   NOT NULL,
    [SyncTypeCode]   SMALLINT        CONSTRAINT [DF_Object_tbFlow_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OffsetDays]     SMALLINT        CONSTRAINT [DF_Object_tbFlow_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]     NVARCHAR (50)   CONSTRAINT [DF_tbTemplateObject_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME        CONSTRAINT [DF_tbTemplateObject_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   CONSTRAINT [DF_tbTemplateObject_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME        CONSTRAINT [DF_tbTemplateObject_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [UsedOnQuantity] DECIMAL (18, 6) CONSTRAINT [DF_Object_tbFlow_UsedOnQuantity] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Object_tbFlow] PRIMARY KEY NONCLUSTERED ([ParentCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Object_tbFlow_Object_tbChild] FOREIGN KEY ([ChildCode]) REFERENCES [Object].[tbObject] ([ObjectCode]),
    CONSTRAINT [FK_Object_tbFlow_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Object_tbFlow_tbObjectParent] FOREIGN KEY ([ParentCode]) REFERENCES [Object].[tbObject] ([ObjectCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbFlow_ChildParent]
    ON [Object].[tbFlow]([ChildCode] ASC, [ParentCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbFlow_ParentChild]
    ON [Object].[tbFlow]([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Object.Object_tbFlow_TriggerUpdate 
   ON  Object.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY		
		UPDATE Object.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentCode = i.ParentCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
