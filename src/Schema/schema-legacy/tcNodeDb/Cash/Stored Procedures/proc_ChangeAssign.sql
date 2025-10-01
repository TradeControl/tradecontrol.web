CREATE   PROCEDURE Cash.proc_ChangeAssign
(
	@CashAccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@PaymentAddress nvarchar(42), 
	@InvoiceNumber nvarchar(20),
	@Note nvarchar(256) = NULL
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRAN

		UPDATE Cash.tbChange
		SET Note = @Note
		WHERE PaymentAddress = @PaymentAddress;

		IF EXISTS (SELECT * FROM Invoice.tbInvoice inv 
						JOIN Invoice.tbType typ ON inv.InvoiceTypeCode = typ.InvoiceTypeCode  
						WHERE typ.CashModeCode = 1 AND InvoiceNumber = @InvoiceNumber AND inv.InvoiceStatusCode BETWEEN 1 AND 2)
		BEGIN
			IF EXISTS (SELECT * FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber)
				DELETE FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber;

			INSERT INTO Cash.tbChangeReference (PaymentAddress, InvoiceNumber)
			VALUES (@PaymentAddress, @InvoiceNumber);

			DECLARE @TxId nvarchar(64);
			DECLARE txIds CURSOR LOCAL FOR
				SELECT TxId FROM Cash.tbTx tx
				WHERE TxStatusCode = 0 AND tx.PaymentAddress = @PaymentAddress;

			OPEN txIds;
			FETCH NEXT FROM txIds INTO @TxId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				EXEC Cash.proc_TxInvoice @PaymentAddress, @TxId;
				FETCH NEXT FROM txIds INTO @TxId
			END

			CLOSE txIds;
			DEALLOCATE txIds;

		END

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
