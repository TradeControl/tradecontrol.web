CREATE   PROCEDURE dbo.AspNetGetUserName(@Id nvarchar(450), @UserName nvarchar(50) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		SELECT @UserName = u.UserName
		FROM AspNetUsers asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress
		WHERE asp.Id = @Id;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
