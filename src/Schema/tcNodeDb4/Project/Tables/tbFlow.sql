CREATE TABLE [Project].[tbFlow] (
    [ParentProjectCode] NVARCHAR (20)   NOT NULL,
    [StepNumber]     SMALLINT        CONSTRAINT [DF_Project_tbFlow_StepNumber] DEFAULT ((10)) NOT NULL,
    [ChildProjectCode]  NVARCHAR (20)   NULL,
    [SyncTypeCode]   SMALLINT        CONSTRAINT [DF_Project_tbFlow_SyncTypeCode] DEFAULT ((0)) NOT NULL,
    [OffsetDays]     REAL            CONSTRAINT [DF_Project_tbFlow_OffsetDays] DEFAULT ((0)) NOT NULL,
    [InsertedBy]     NVARCHAR (50)   CONSTRAINT [DF_Project_tbFlow_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME        CONSTRAINT [DF_Project_tbFlow_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   CONSTRAINT [DF_Project_tbFlow_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME        CONSTRAINT [DF_Project_tbFlow_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [UsedOnQuantity] DECIMAL (18, 6) CONSTRAINT [DF_Project_tbFlow_UsedOnQuantity] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Project_tbFlow] PRIMARY KEY CLUSTERED ([ParentProjectCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Project_tbFlow_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]),
    CONSTRAINT [FK_Project_tbFlow_Project_tb_Child] FOREIGN KEY ([ChildProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]),
    CONSTRAINT [FK_Project_tbFlow_Project_tb_Parent] FOREIGN KEY ([ParentProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbFlow_ChildParent]
    ON [Project].[tbFlow]([ChildProjectCode] ASC, [ParentProjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbFlow_ParentChild]
    ON [Project].[tbFlow]([ParentProjectCode] ASC, [ChildProjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Project.Project_tbFlow_TriggerUpdate 
   ON  Project.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentProjectCode = i.ParentProjectCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
