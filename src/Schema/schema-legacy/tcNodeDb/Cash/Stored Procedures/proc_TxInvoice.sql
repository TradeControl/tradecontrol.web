CREATE   PROCEDURE Cash.proc_TxInvoice (@PaymentAddress nvarchar(42), @TxId nvarchar(64))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

		DECLARE @PaymentCode nvarchar(20);

		IF EXISTS (
				SELECT * 
				FROM Cash.tbTx 
				WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress AND TxStatusCode = 0
			)
			AND EXISTS (
				SELECT * 
				FROM Cash.tbTx tx
					JOIN Cash.tbChangeReference ref ON tx.PaymentAddress = ref.PaymentAddress 
					JOIN Invoice.tbInvoice inv ON ref.InvoiceNumber = inv.InvoiceNumber 
				WHERE tx.TxId = @TxId AND inv.InvoiceStatusCode < 3	
			)
			AND NOT EXISTS (
				SELECT * 
				FROM Cash.tbTxReference ref 
					JOIN Cash.tbTx tx ON tx.TxNumber = ref.TxNumber WHERE tx.TxId = @TxId AND tx.PaymentAddress = @PaymentAddress
			)		
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode output;

			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaymentStatusCode, AccountCode, CashAccountCode, PaidInValue, PaymentReference)
			SELECT @PaymentCode PaymentCode, (SELECT UserId FROM Usr.vwCredentials) UserId, 0 PaymentStatusCode, invoice.AccountCode, change.CashAccountCode, tx.MoneyIn - tx.MoneyOut PaidInValue, invoice.InvoiceNumber
			FROM Cash.tbTx tx
				JOIN Cash.tbChange change ON tx.PaymentAddress = change.PaymentAddress
				JOIN Cash.tbChangeReference ref ON change.PaymentAddress = ref.PaymentAddress
				JOIN Invoice.tbInvoice invoice ON ref.InvoiceNumber = invoice.InvoiceNumber
			WHERE tx.TxId = @TxId;

			UPDATE Cash.tbTx
			SET TxStatusCode = 1
			WHERE TxId = @TxId;

			INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
			SELECT TxNumber, TxStatusCode, @PaymentCode PaymentCode
			FROM Cash.tbTx
			WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

			Exec Cash.proc_PaymentPost;

		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
