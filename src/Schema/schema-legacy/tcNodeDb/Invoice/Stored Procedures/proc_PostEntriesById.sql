CREATE   PROCEDURE Invoice.proc_PostEntriesById(@UserId nvarchar(10))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@AccountCode nvarchar(10)
			, @InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			
		DECLARE c1 CURSOR LOCAL FOR
			SELECT AccountCode, InvoiceTypeCode
			FROM Invoice.tbEntry
			WHERE UserId = @UserId
			GROUP BY AccountCode, InvoiceTypeCode;

		OPEN c1;

		BEGIN TRAN;

		FETCH NEXT FROM c1 INTO @AccountCode, @InvoiceTypeCode;
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Invoice.proc_RaiseBlank @AccountCode, @InvoiceTypeCode, @InvoiceNumber output;

			WITH invoice_entry AS
			(
				SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
				FROM Invoice.tbEntry
				WHERE AccountCode = @AccountCode AND InvoiceTypeCode = @InvoiceTypeCode
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
			WHERE AccountCode = @AccountCode AND InvoiceTypeCode = @InvoiceTypeCode

			EXEC Invoice.proc_Accept @InvoiceNumber;

			FETCH NEXT FROM c1 INTO @AccountCode, @InvoiceTypeCode;
		END

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId;

		COMMIT TRAN;

		CLOSE c1;
		DEALLOCATE c1;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
