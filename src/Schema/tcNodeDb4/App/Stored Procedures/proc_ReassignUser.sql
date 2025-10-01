
CREATE   PROCEDURE App.proc_ReassignUser 
	(
	@UserId nvarchar(10)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE    Usr.tbUser
		SET       LogonName = (SUSER_SNAME())
		WHERE     (UserId = @UserId)
	
   	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
