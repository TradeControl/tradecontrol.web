CREATE TABLE [Task].[tbFlow] (
    [ParentTaskCode] NVARCHAR (20)   NOT NULL,
    [StepNumber]     SMALLINT        CONSTRAINT [DF_Task_tbFlow_StepNumber] DEFAULT ((10)) NOT NULL,
    [ChildTaskCode]  NVARCHAR (20)   NULL,
    [SyncTypeCode]   SMALLINT        CONSTRAINT [DF_Task_tbFlow_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OffsetDays]     REAL            CONSTRAINT [DF_Task_tbFlow_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]     NVARCHAR (50)   CONSTRAINT [DF_Task_tbFlow_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME        CONSTRAINT [DF_Task_tbFlow_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   CONSTRAINT [DF_Task_tbFlow_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME        CONSTRAINT [DF_Task_tbFlow_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [UsedOnQuantity] DECIMAL (18, 6) CONSTRAINT [DF_Task_tbFlow_UsedOnQuantity] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Task_tbFlow] PRIMARY KEY CLUSTERED ([ParentTaskCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Task_tbFlow_Activity_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Activity].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Task_tbFlow_Task_tb_Child] FOREIGN KEY ([ChildTaskCode]) REFERENCES [Task].[tbTask] ([TaskCode]),
    CONSTRAINT [FK_Task_tbFlow_Task_tb_Parent] FOREIGN KEY ([ParentTaskCode]) REFERENCES [Task].[tbTask] ([TaskCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Task_tbFlow_ChildParent]
    ON [Task].[tbFlow]([ChildTaskCode] ASC, [ParentTaskCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Task_tbFlow_ParentChild]
    ON [Task].[tbFlow]([ParentTaskCode] ASC, [ChildTaskCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Task.Task_tbFlow_TriggerUpdate 
   ON  Task.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Task.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentTaskCode = i.ParentTaskCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
