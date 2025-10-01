CREATE   PROCEDURE dbo.AspNetGetId(@UserId nvarchar(10), @Id nvarchar(450) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		SELECT @Id = UserId 
		FROM AspNetUsers asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress
		WHERE u.UserId = @UserId;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
