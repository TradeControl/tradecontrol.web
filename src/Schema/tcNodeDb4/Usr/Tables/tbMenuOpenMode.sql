CREATE TABLE [Usr].[tbMenuOpenMode] (
    [OpenMode]            SMALLINT      CONSTRAINT [DF_Usr_tbMenuOpenMode_OpenMode] DEFAULT ((0)) NOT NULL,
    [OpenModeDescription] NVARCHAR (20) NULL,
    CONSTRAINT [PK_Usr_tbMenuOpenMode] PRIMARY KEY CLUSTERED ([OpenMode] ASC) WITH (FILLFACTOR = 90)
);

