
CREATE   PROCEDURE Usr.proc_MenuInsert
	(
		@MenuName nvarchar(50),
		@FromMenuId smallint = 0,
		@MenuId smallint = null OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRANSACTION
	
		INSERT INTO Usr.tbMenu (MenuName) VALUES (@MenuName)
		SELECT @MenuId = @@IDENTITY
	
		IF @FromMenuId = 0
			BEGIN
			INSERT INTO Usr.tbMenuEntry (MenuId, FolderId, ItemId, ItemText, Command,  Argument)
					VALUES (@MenuId, 1, 0, @MenuName, 0, 'Root')
			END
		ELSE
			BEGIN
			INSERT INTO Usr.tbMenuEntry
								  (MenuId, FolderId, ItemId, OpenMode, Argument, ProjectName, Command, ItemText)
			SELECT     @MenuId AS ToMenuId, FolderId, ItemId, OpenMode, Argument, ProjectName, Command, ItemText
			FROM         Usr.tbMenuEntry
			WHERE     (MenuId = @FromMenuId)
			END

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
