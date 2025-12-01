CREATE TABLE [Project].[tbOpStatus] (
    [OpStatusCode] SMALLINT      NOT NULL,
    [OpStatus]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Project_tbOpStatus] PRIMARY KEY CLUSTERED ([OpStatusCode] ASC) WITH (FILLFACTOR = 90)
);

