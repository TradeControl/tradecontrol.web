CREATE TABLE [Activity].[tbFlow] (
    [ParentCode]     NVARCHAR (50)   NOT NULL,
    [StepNumber]     SMALLINT        CONSTRAINT [DF_Activity_tbFlow_StepNumber] DEFAULT ((10)) NOT NULL,
    [ChildCode]      NVARCHAR (50)   NOT NULL,
    [SyncTypeCode]   SMALLINT        CONSTRAINT [DF_Activity_tbFlow_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OffsetDays]     SMALLINT        CONSTRAINT [DF_Activity_tbFlow_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]     NVARCHAR (50)   CONSTRAINT [DF_tbTemplateActivity_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME        CONSTRAINT [DF_tbTemplateActivity_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   CONSTRAINT [DF_tbTemplateActivity_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME        CONSTRAINT [DF_tbTemplateActivity_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [UsedOnQuantity] DECIMAL (18, 6) CONSTRAINT [DF_Activity_tbFlow_UsedOnQuantity] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Activity_tbFlow] PRIMARY KEY NONCLUSTERED ([ParentCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Activity_tbFlow_Activity_tbChild] FOREIGN KEY ([ChildCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode]),
    CONSTRAINT [FK_Activity_tbFlow_Activity_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Activity].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Activity_tbFlow_tbActivityParent] FOREIGN KEY ([ParentCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Activity_tbFlow_ChildParent]
    ON [Activity].[tbFlow]([ChildCode] ASC, [ParentCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Activity_tbFlow_ParentChild]
    ON [Activity].[tbFlow]([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Activity.Activity_tbFlow_TriggerUpdate 
   ON  Activity.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY		
		UPDATE Activity.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Activity.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentCode = i.ParentCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
