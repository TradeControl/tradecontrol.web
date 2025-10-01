CREATE TABLE [App].[tbCodeExclusion] (
    [ExcludedTag] NVARCHAR (100) NOT NULL,
    [RowVer]      ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbCodeExclusion] PRIMARY KEY CLUSTERED ([ExcludedTag] ASC) WITH (FILLFACTOR = 90)
);

