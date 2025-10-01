
CREATE   PROCEDURE Usr.proc_MenuItemDelete( @EntryId int )
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRANSACTION

		DECLARE @MenuId SMALLINT = (SELECT MenuId FROM Usr.tbMenuEntry menu WHERE menu.EntryId = @EntryId);

		DELETE FROM Usr.tbMenuEntry
		WHERE Command = 1 
			AND MenuId = @MenuId
			AND Argument = (SELECT FolderId FROM Usr.tbMenuEntry menu WHERE Command = 0 AND menu.EntryId = @EntryId);

		 WITH root_folder AS
		 (
			 SELECT FolderId, MenuId 
			 FROM Usr.tbMenuEntry menu
			 WHERE Command = 0 AND menu.EntryId = @EntryId
		), child_folders AS
		(
			SELECT CAST(Argument AS smallint) AS FolderId, root_folder.MenuId
			FROM Usr.tbMenuEntry sub_folder 
			JOIN root_folder ON sub_folder.FolderId = root_folder.FolderId
			WHERE Command = 1 AND sub_folder.MenuId = @MenuId

			UNION ALL

			SELECT CAST(Argument AS smallint) AS FolderId, p.MenuId
			FROM child_folders p 
				JOIN Usr.tbMenuEntry m ON p.FolderId = m.FolderId
			WHERE Command = 1 AND m.MenuId = p.MenuId
		), folders AS
		(
			select FolderId from root_folder
			UNION
			select FolderId from child_folders
		)
		DELETE Usr.tbMenuEntry 
		FROM Usr.tbMenuEntry JOIN folders ON Usr.tbMenuEntry.FolderId = folders.FolderId

		DELETE FROM Usr.tbMenuEntry WHERE EntryId = @EntryId;

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
