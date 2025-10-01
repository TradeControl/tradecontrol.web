CREATE TABLE [Task].[tbCostSet] (
    [TaskCode]   NVARCHAR (20) NOT NULL,
    [UserId]     NVARCHAR (10) NOT NULL,
    [InsertedBy] NVARCHAR (50) CONSTRAINT [Task_tbCostSet_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn] DATETIME      CONSTRAINT [Task_tbCostSet_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Task_tbCostSet] PRIMARY KEY CLUSTERED ([TaskCode] ASC, [UserId] ASC),
    CONSTRAINT [FK_Task_tbCostSet_Task_tbTask] FOREIGN KEY ([TaskCode]) REFERENCES [Task].[tbTask] ([TaskCode]) ON DELETE CASCADE,
    CONSTRAINT [FK_Task_tbCostSet_Usr_tbUser] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Task_tbCostSet_UserId]
    ON [Task].[tbCostSet]([UserId] ASC, [TaskCode] ASC);

