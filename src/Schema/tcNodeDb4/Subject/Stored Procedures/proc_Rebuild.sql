CREATE PROCEDURE Subject.proc_Rebuild(@SubjectCode NVARCHAR(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	DECLARE @PaymentCode nvarchar(20);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE Invoice.tbItem
		SET 
			InvoiceValue =  ROUND(Invoice.tbItem.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbItem.TotalValue <> 0
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);

		UPDATE Invoice.tbItem
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0)
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);
                   
		UPDATE Invoice.tbProject
		SET InvoiceValue =  ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbProject.TotalValue <> 0
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);

		UPDATE Invoice.tbProject
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END,
			InvoiceValue = CASE WHEN Invoice.tbProject.TotalValue = 0 
								THEN Invoice.tbProject.InvoiceValue 
								ELSE ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), 2) 
							END
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0)
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);
						   	
		WITH items AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbItem.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbItem.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbItem INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), Projects AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbProject.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbProject.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbProject INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), invoice_totals AS
		(
			SELECT invoices.InvoiceNumber, 
				COALESCE(items.TotalInvoiceValue, 0) + COALESCE(Projects.TotalInvoiceValue, 0) TotalInvoiceValue,
				COALESCE(items.TotalTaxValue, 0) + COALESCE(Projects.TotalTaxValue, 0) TotalTaxValue
			FROM Invoice.tbInvoice invoices
				LEFT OUTER JOIN Projects ON invoices.InvoiceNumber = Projects.InvoiceNumber
				LEFT OUTER JOIN items ON invoices.InvoiceNumber = items.InvoiceNumber
			WHERE ( invoices.InvoiceStatusCode > 0)
		)
		UPDATE invoices
		SET InvoiceValue = TotalInvoiceValue, 
			TaxValue = TotalTaxValue
		FROM  Invoice.tbInvoice invoices 
			JOIN invoice_totals ON invoices.InvoiceNumber = invoice_totals.InvoiceNumber
		WHERE SubjectCode = @SubjectCode AND (InvoiceValue <> TotalInvoiceValue OR TaxValue <> TotalTaxValue);



		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
			WHERE SubjectCode = @SubjectCode
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

		COMMIT TRANSACTION

		DECLARE @Msg NVARCHAR(MAX);
		SELECT @Msg = CONCAT(@SubjectCode, ' ', Message) FROM App.tbText WHERE TextId = 3006;
		EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 2;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
