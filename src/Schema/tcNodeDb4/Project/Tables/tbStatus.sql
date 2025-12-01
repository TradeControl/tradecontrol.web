CREATE TABLE [Project].[tbStatus] (
    [ProjectStatusCode] SMALLINT       NOT NULL,
    [ProjectStatus]     NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Project_tbStatus] PRIMARY KEY NONCLUSTERED ([ProjectStatusCode] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbStatus_ProjectStatus]
    ON [Project].[tbStatus]([ProjectStatus] ASC) WITH (FILLFACTOR = 90);

