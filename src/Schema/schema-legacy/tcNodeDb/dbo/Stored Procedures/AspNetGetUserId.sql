CREATE   PROCEDURE dbo.AspNetGetUserId(@Id nvarchar(450), @UserId nvarchar(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		WITH asp AS
		(
			SELECT Id, UserName
			FROM AspNetUsers 
			WHERE Id = @Id
		)
		SELECT @UserId = UserId 
		FROM asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
