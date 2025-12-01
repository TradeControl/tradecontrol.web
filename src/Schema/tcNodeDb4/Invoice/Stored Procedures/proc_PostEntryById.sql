CREATE   PROCEDURE Invoice.proc_PostEntryById(@UserId nvarchar(10), @SubjectCode nvarchar(10), @CashCode nvarchar(50))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			

		BEGIN TRAN;

		SELECT @InvoiceTypeCode = InvoiceTypeCode 
		FROM Invoice.tbEntry 
		WHERE UserId = @UserId AND SubjectCode = @SubjectCode AND CashCode = @CashCode;
		
		EXEC Invoice.proc_RaiseBlank @SubjectCode, @InvoiceTypeCode, @InvoiceNumber output;

		WITH invoice_entry AS
		(
			SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
			FROM Invoice.tbEntry
			WHERE SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode
		)
		UPDATE Invoice.tbInvoice
		SET 
			UserId = @UserId,
			InvoicedOn = invoice_entry.InvoicedOn,
			Printed = CASE WHEN  @InvoiceTypeCode < 2 THEN 0 ELSE 1 END
		FROM Invoice.tbInvoice invoice_header 
			JOIN invoice_entry ON invoice_header.InvoiceNumber = invoice_entry.InvoiceNumber;

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue)
		SELECT @InvoiceNumber InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue
		FROM Invoice.tbEntry
		WHERE SubjectCode = @SubjectCode AND CashCode = @CashCode

		EXEC Invoice.proc_Accept @InvoiceNumber;

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId AND SubjectCode = @SubjectCode AND CashCode = @CashCode;

		COMMIT TRAN;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
