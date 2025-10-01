CREATE TABLE [App].[tbDoc] (
    [DocTypeCode] SMALLINT      NOT NULL,
    [ReportName]  NVARCHAR (50) NOT NULL,
    [OpenMode]    SMALLINT      CONSTRAINT [DF_App_tbDoc_OpenMode] DEFAULT ((1)) NOT NULL,
    [Description] NVARCHAR (50) NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbDoc] PRIMARY KEY CLUSTERED ([DocTypeCode] ASC, [ReportName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbDoc_Usr_tbMenuOpenMode] FOREIGN KEY ([OpenMode]) REFERENCES [Usr].[tbMenuOpenMode] ([OpenMode])
);

