CREATE TABLE [Subject].[tbSector] (
    [SubjectCode]    NVARCHAR (10) NOT NULL,
    [IndustrySector] NVARCHAR (50) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbSector] PRIMARY KEY CLUSTERED ([SubjectCode] ASC, [IndustrySector] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbSector_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbSector_IndustrySector]
    ON [Subject].[tbSector]([IndustrySector] ASC) WITH (FILLFACTOR = 90);

