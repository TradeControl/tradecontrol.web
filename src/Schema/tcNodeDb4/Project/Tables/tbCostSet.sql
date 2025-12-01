CREATE TABLE [Project].[tbCostSet] (
    [ProjectCode]   NVARCHAR (20) NOT NULL,
    [UserId]     NVARCHAR (10) NOT NULL,
    [InsertedBy] NVARCHAR (50) CONSTRAINT [Project_tbCostSet_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn] DATETIME      CONSTRAINT [Project_tbCostSet_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Project_tbCostSet] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [UserId] ASC),
    CONSTRAINT [FK_Project_tbCostSet_Project_tbProject] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE,
    CONSTRAINT [FK_Project_tbCostSet_Usr_tbUser] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbCostSet_UserId]
    ON [Project].[tbCostSet]([UserId] ASC, [ProjectCode] ASC);

