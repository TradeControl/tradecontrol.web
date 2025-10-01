CREATE PROCEDURE Invoice.proc_Cancel
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		DECLARE @UserId nvarchar(10) = (SELECT TOP 1 UserId FROM Usr.vwCredentials)
		EXEC Invoice.proc_CancelById @UserId

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
