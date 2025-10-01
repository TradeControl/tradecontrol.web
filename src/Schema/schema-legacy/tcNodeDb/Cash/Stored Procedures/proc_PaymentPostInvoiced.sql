CREATE PROCEDURE Cash.proc_PaymentPostInvoiced (@PaymentCode nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@AccountCode nvarchar(10)
			, @PostValue decimal(18, 5)
			, @CashCode nvarchar(50);

		SELECT   @PostValue = CASE WHEN PaidInValue = 0 THEN PaidOutValue ELSE PaidInValue * -1 END,
			@AccountCode = Org.tbOrg.AccountCode
		FROM         Cash.tbPayment INNER JOIN
							  Org.tbOrg ON Cash.tbPayment.AccountCode = Org.tbOrg.AccountCode
		WHERE     ( Cash.tbPayment.PaymentCode = @PaymentCode);

		IF NOT EXISTS (SELECT InvoiceNumber FROM Invoice.tbInvoice WHERE (InvoiceStatusCode BETWEEN 1 AND 2) AND (AccountCode = @AccountCode))
			RETURN;

		IF EXISTS (SELECT * FROM  Invoice.tbInvoice 
						INNER JOIN Invoice.tbTask ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbTask.InvoiceNumber
					WHERE        (Invoice.tbInvoice.AccountCode = @AccountCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3))
		BEGIN
			SELECT  @CashCode = Invoice.tbTask.CashCode
			FROM  Invoice.tbInvoice 
				INNER JOIN Invoice.tbTask ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbTask.InvoiceNumber
			WHERE        (Invoice.tbInvoice.AccountCode = @AccountCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
			GROUP BY Invoice.tbTask.CashCode;
		END
		ELSE IF EXISTS (SELECT * FROM Invoice.tbInvoice 
							INNER JOIN Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber
						WHERE        (Invoice.tbInvoice.AccountCode = @AccountCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
						GROUP BY Invoice.tbItem.CashCode)
		BEGIN
			SELECT @CashCode = Invoice.tbItem.CashCode
			FROM  Invoice.tbInvoice 
				INNER JOIN Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber
			WHERE        (Invoice.tbInvoice.AccountCode = @AccountCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
			GROUP BY Invoice.tbItem.CashCode;
		END

		BEGIN TRANSACTION;

		UPDATE Cash.tbPayment
		SET PaymentStatusCode = 1, CashCode = @CashCode
		WHERE (PaymentCode = @PaymentCode);
		
		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
			WHERE AccountCode = @AccountCode
		)
		UPDATE invoices
		SET 
			InvoiceStatusCode = invoice_status.InvoiceStatusCode,
			PaidValue = invoice_status.PaidValue,
			PaidTaxValue = invoice_status.PaidTaxValue
		FROM Invoice.tbInvoice invoices	
			JOIN invoice_status ON invoices.InvoiceNumber = invoice_status.InvoiceNumber
		WHERE 
			invoices.InvoiceStatusCode <> invoice_status.InvoiceStatusCode 
			OR invoices.PaidValue <> invoice_status.PaidValue 
			OR invoices.PaidTaxValue <> invoice_status.PaidTaxValue;

		UPDATE  Org.tbAccount
		SET CurrentBalance = Org.tbAccount.CurrentBalance + (@PostValue * -1)
		FROM         Org.tbAccount INNER JOIN
							  Cash.tbPayment ON Org.tbAccount.CashAccountCode = Cash.tbPayment.CashAccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode
		
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
