CREATE   PROCEDURE Cash.proc_ChangeNew 
(
	@CashAccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@ChangeTypeCode smallint,
	@PaymentAddress nvarchar(42), 
	@AddressIndex int = 0, 
	@InvoiceNumber nvarchar(20) = NULL,
	@Note nvarchar(256) = NULL
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRAN

		INSERT INTO Cash.tbChange (PaymentAddress, CashAccountCode, HDPath, ChangeTypeCode, AddressIndex, Note)
		SELECT @PaymentAddress, @CashAccountCode, account_key.HDPath, @ChangeTypeCode, @AddressIndex, @Note
		FROM Org.tbAccountKey account_key
		WHERE account_key.CashAccountCode = @CashAccountCode AND KeyName = @KeyName;

		IF EXISTS (SELECT * FROM Invoice.tbInvoice inv 
						JOIN Invoice.tbType typ ON inv.InvoiceTypeCode = typ.InvoiceTypeCode  
						WHERE typ.CashModeCode = 1 AND InvoiceNumber = @InvoiceNumber)
		BEGIN
			IF EXISTS (SELECT * FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber)
				DELETE FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber;
			INSERT INTO Cash.tbChangeReference (PaymentAddress, InvoiceNumber)
			VALUES (@PaymentAddress, @InvoiceNumber);
		END

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
