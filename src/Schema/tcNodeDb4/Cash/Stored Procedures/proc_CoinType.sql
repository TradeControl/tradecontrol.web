CREATE   PROCEDURE Cash.proc_CoinType(@CoinTypeCode smallint output)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		DECLARE @AccountCode nvarchar(10);

		EXEC Cash.proc_CurrentAccount @AccountCode output
		SELECT @CoinTypeCode = CoinTypeCode FROM Subject.tbAccount WHERE AccountCode = @AccountCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
