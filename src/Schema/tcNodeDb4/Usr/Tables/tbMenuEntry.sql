CREATE TABLE [Usr].[tbMenuEntry] (
    [MenuId]      SMALLINT       CONSTRAINT [DF_Usr_tbMenuEntry_MenuId] DEFAULT ((0)) NOT NULL,
    [EntryId]     INT            IDENTITY (1, 1) NOT NULL,
    [FolderId]    SMALLINT       CONSTRAINT [DF_Usr_tbMenuEntry_FolderId] DEFAULT ((0)) NOT NULL,
    [ItemId]      SMALLINT       CONSTRAINT [DF_Usr_tbMenuEntry_ItemId] DEFAULT ((0)) NOT NULL,
    [ItemText]    NVARCHAR (255) NULL,
    [Command]     SMALLINT       CONSTRAINT [DF_Usr_tbMenuEntry_Command] DEFAULT ((0)) NULL,
    [ProjectName] NVARCHAR (50)  NULL,
    [Argument]    NVARCHAR (50)  NULL,
    [OpenMode]    SMALLINT       CONSTRAINT [DF_Usr_tbMenuEntry_OpenMode] DEFAULT ((1)) NULL,
    [UpdatedOn]   DATETIME       CONSTRAINT [DF_Usr_tbMenuEntry_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [InsertedOn]  DATETIME       CONSTRAINT [DF_Usr_tbMenuEntry_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]   NVARCHAR (50)  CONSTRAINT [DF_Usr_tbMenuEntry_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [RowVer]      ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Usr_tbMenuEntry] PRIMARY KEY CLUSTERED ([MenuId] ASC, [EntryId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Usr_tbMenuEntry_Usr_tbMenu] FOREIGN KEY ([MenuId]) REFERENCES [Usr].[tbMenu] ([MenuId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Usr_tbMenuEntry_tbMenuCommand] FOREIGN KEY ([Command]) REFERENCES [Usr].[tbMenuCommand] ([Command]),
    CONSTRAINT [FK_Usr_tbMenuEntry_tbMenuOpenMode] FOREIGN KEY ([OpenMode]) REFERENCES [Usr].[tbMenuOpenMode] ([OpenMode]),
    CONSTRAINT [IX_Usr_tbMenuEntry_MenuFolderItem] UNIQUE NONCLUSTERED ([MenuId] ASC, [FolderId] ASC, [ItemId] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_Usr_tbMenuEntry_Command]
    ON [Usr].[tbMenuEntry]([Command] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Usr_tbMenuEntry_OpenMode]
    ON [Usr].[tbMenuEntry]([OpenMode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Usr.Usr_tbMenuEntry_TriggerUpdate 
   ON  Usr.tbMenuEntry
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Usr.tbMenuEntry
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Usr.tbMenuEntry INNER JOIN inserted AS i ON tbMenuEntry.EntryId = i.EntryId AND tbMenuEntry.EntryId = i.EntryId;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
