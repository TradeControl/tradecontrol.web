CREATE TABLE [Subject].[tbStatus] (
    [SubjectStatusCode] SMALLINT       CONSTRAINT [DF_Subject_tbStatus_SubjectStatusCode] DEFAULT ((1)) NOT NULL,
    [SubjectStatus]     NVARCHAR (255) NULL,
    CONSTRAINT [PK_Subject_tbStatus] PRIMARY KEY NONCLUSTERED ([SubjectStatusCode] ASC) WITH (FILLFACTOR = 90)
);

