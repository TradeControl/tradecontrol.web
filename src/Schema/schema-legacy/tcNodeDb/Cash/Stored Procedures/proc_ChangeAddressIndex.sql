CREATE   PROCEDURE Cash.proc_ChangeAddressIndex 
(
	@CashAccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@ChangeTypeCode smallint,
	@AddressIndex int = 0 output
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

		SELECT @AddressIndex = COALESCE(MAX(change.AddressIndex) + 1, 0) 
		FROM Cash.tbChange change
			JOIN Org.tbAccountKey account_key ON change.CashAccountCode = account_key.CashAccountCode AND change.HDPath = account_key.HDPath
		WHERE account_key.CashAccountCode = @CashAccountCode AND KeyName = @KeyName AND change.ChangeTypeCode = @ChangeTypeCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
