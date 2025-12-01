CREATE PROCEDURE Cash.proc_ReserveAccount(@AccountCode NVARCHAR(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		SELECT  @AccountCode = AccountCode
		FROM Cash.vwReserveAccount;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
