CREATE   PROCEDURE Cash.proc_Mirror(@CashCode nvarchar(50), @SubjectCode nvarchar(10), @ChargeCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Cash.tbMirror WHERE CashCode = @CashCode AND SubjectCode = @SubjectCode AND ChargeCode = @ChargeCode)
		BEGIN
			INSERT INTO Cash.tbMirror (CashCode, SubjectCode, ChargeCode)
			VALUES (@CashCode, @SubjectCode, @ChargeCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
