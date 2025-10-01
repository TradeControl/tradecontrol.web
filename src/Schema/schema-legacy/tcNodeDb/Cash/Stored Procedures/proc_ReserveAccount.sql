CREATE PROCEDURE Cash.proc_ReserveAccount(@CashAccountCode NVARCHAR(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		SELECT  @CashAccountCode = CashAccountCode
		FROM Cash.vwReserveAccount;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
