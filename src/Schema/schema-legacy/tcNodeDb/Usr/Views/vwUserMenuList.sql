CREATE   VIEW Usr.vwUserMenuList
AS
	WITH user_menus AS
	(
		SELECT MenuId
		FROM Usr.tbMenuUser
		WHERE UserId = (SELECT UserId FROM Usr.vwCredentials)
	), folders AS
	(
		SELECT folder.MenuId, folder.Argument FolderId , folder.ItemText 
			, (SELECT parent_folder.FolderId FROM Usr.tbMenuEntry parent_folder WHERE parent_folder.MenuId = folder.MenuId and parent_folder.FolderId = folder.FolderId and Command = 0) ParentFolderId 
		FROM Usr.tbMenuEntry folder
			JOIN user_menus ON folder.MenuId = user_menus.MenuId
		WHERE Command = 1
	), return_commands AS
	(
		SELECT folders.MenuId, folders.FolderId,
			(SELECT MAX(ItemId) + 1 FROM Usr.tbMenuEntry WHERE MenuId = folders.MenuId and FolderId = folders.FolderId) ItemId,
			(SELECT CONCAT('Return to ', CASE Argument WHEN 'Root' THEN 'Main Menu' ELSE ItemText END) FROM Usr.tbMenuEntry WHERE MenuId = folders.MenuId and FolderId = folders.ParentFolderId and ItemId = 0) ItemText,
			CAST(1 AS smallint) Command,
			NULL ProjectName,
			CAST(ParentFolderId as nvarchar(50)) Argument,
			CAST(0 AS smallint) OpenMode
		FROM folders
	), menu_items AS
	(
		SELECT menu_entries.MenuId, FolderId, 
			ROW_NUMBER() OVER (PARTITION BY menu_entries.MenuId, FolderId ORDER BY ItemText DESC) RowNumber,
			ItemId, ItemText, Command, ProjectName, Argument, OpenMode
		FROM Usr.tbMenuEntry menu_entries
			JOIN user_menus ON menu_entries.MenuId = user_menus.MenuId
		UNION
		SELECT MenuId, FolderId, 0 RowNumber, ItemId, ItemText, Command, ProjectName, Argument, OpenMode
		FROM return_commands
	)
	SELECT menu.MenuId, menu.InterfaceCode, FolderId, RowNumber, ItemId, ItemText, Command, ProjectName, Argument, OpenMode
	FROM menu_items
		JOIN Usr.tbMenu menu ON menu_items.MenuId = menu.MenuId;
