CREATE   PROCEDURE Cash.proc_CoinType(@CoinTypeCode smallint output)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		DECLARE @CashAccountCode nvarchar(10);

		EXEC Cash.proc_CurrentAccount @CashAccountCode output
		SELECT @CoinTypeCode = CoinTypeCode FROM Org.tbAccount WHERE CashAccountCode = @CashAccountCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
