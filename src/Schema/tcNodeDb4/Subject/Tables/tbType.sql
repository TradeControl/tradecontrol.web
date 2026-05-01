CREATE TABLE [Subject].[tbType] (
    [SubjectTypeCode]  SMALLINT      CONSTRAINT [DF_Subject_tbType_SubjectTypeCode] DEFAULT ((1)) NOT NULL,
    [CashPolarityCode] SMALLINT      NOT NULL,
    [SubjectClassCode] SMALLINT      CONSTRAINT [DF_Subject_tbType_SubjectClassCode] DEFAULT ((0)) NOT NULL,
    [SubjectType]      NVARCHAR (50) NOT NULL,
    [RowVer]           ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbType] PRIMARY KEY NONCLUSTERED ([SubjectTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbType_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]),
    CONSTRAINT [FK_Subject_tbType_tbClass] FOREIGN KEY ([SubjectClassCode]) REFERENCES [Subject].[tbClass] ([SubjectClassCode])
);

