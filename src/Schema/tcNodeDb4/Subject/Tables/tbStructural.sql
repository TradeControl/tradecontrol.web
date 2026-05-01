CREATE TABLE [Subject].[tbStructural] (
    [SubjectCode] NVARCHAR (50) NOT NULL,
    [Notes]       NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Subject_tbStructural] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbStructural_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO
