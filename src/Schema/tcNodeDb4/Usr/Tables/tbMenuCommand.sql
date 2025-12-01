CREATE TABLE [Usr].[tbMenuCommand] (
    [Command]     SMALLINT      CONSTRAINT [DF_Usr_tbMenuCommand_Command] DEFAULT ((0)) NOT NULL,
    [CommandText] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Usr_tbMenuCommand] PRIMARY KEY CLUSTERED ([Command] ASC) WITH (FILLFACTOR = 90)
);

