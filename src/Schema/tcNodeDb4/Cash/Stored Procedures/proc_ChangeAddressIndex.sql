CREATE   PROCEDURE Cash.proc_ChangeAddressIndex 
(
	@AccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@ChangeTypeCode smallint,
	@AddressIndex int = 0 output
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

		SELECT @AddressIndex = COALESCE(MAX(change.AddressIndex) + 1, 0) 
		FROM Cash.tbChange change
			JOIN Subject.tbAccountKey account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath
		WHERE account_key.AccountCode = @AccountCode AND KeyName = @KeyName AND change.ChangeTypeCode = @ChangeTypeCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
