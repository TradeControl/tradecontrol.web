CREATE TABLE [App].[tbText] (
    [TextId]    INT        NOT NULL,
    [Message]   NVARCHAR(MAX)      NOT NULL,
    [Arguments] SMALLINT   NOT NULL,
    [RowVer]    ROWVERSION NOT NULL,
    CONSTRAINT [PK_App_tbText] PRIMARY KEY CLUSTERED ([TextId] ASC) WITH (FILLFACTOR = 90)
);

