
CREATE   PROCEDURE Usr.proc_MenuCleanReferences(@MenuId SMALLINT)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		WITH tbFolderRefs AS 
		(	SELECT        MenuId, EntryId, CAST(Argument AS int) AS FolderIdRef
			FROM            Usr.tbMenuEntry
			WHERE        (Command = 1))
		, tbBadRefs AS
		(
			SELECT        tbFolderRefs.EntryId
			FROM            tbFolderRefs LEFT OUTER JOIN
									Usr.tbMenuEntry AS tbMenuEntry ON tbFolderRefs.FolderIdRef = tbMenuEntry.FolderId AND tbFolderRefs.MenuId = tbMenuEntry.MenuId
			WHERE (tbMenuEntry.MenuId = @MenuId) AND (tbMenuEntry.MenuId IS NULL)
		)
		DELETE FROM Usr.tbMenuEntry
		FROM            Usr.tbMenuEntry INNER JOIN
								 tbBadRefs ON Usr.tbMenuEntry.EntryId = tbBadRefs.EntryId;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
