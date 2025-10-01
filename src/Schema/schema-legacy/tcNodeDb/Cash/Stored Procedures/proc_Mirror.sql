CREATE   PROCEDURE Cash.proc_Mirror(@CashCode nvarchar(50), @AccountCode nvarchar(10), @ChargeCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Cash.tbMirror WHERE CashCode = @CashCode AND AccountCode = @AccountCode AND ChargeCode = @ChargeCode)
		BEGIN
			INSERT INTO Cash.tbMirror (CashCode, AccountCode, ChargeCode)
			VALUES (@CashCode, @AccountCode, @ChargeCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
