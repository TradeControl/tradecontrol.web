CREATE TABLE [Subject].[tbClass] (
    [SubjectClassCode] SMALLINT      NOT NULL,
    [SubjectClass]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Subject_tbClass] PRIMARY KEY CLUSTERED ([SubjectClassCode] ASC) WITH (FILLFACTOR = 90)
);
