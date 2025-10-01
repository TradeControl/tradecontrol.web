CREATE TABLE [Task].[tbStatus] (
    [TaskStatusCode] SMALLINT       NOT NULL,
    [TaskStatus]     NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Task_tbStatus] PRIMARY KEY NONCLUSTERED ([TaskStatusCode] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Task_tbStatus_TaskStatus]
    ON [Task].[tbStatus]([TaskStatus] ASC) WITH (FILLFACTOR = 90);

