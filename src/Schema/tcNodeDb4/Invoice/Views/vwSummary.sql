CREATE VIEW Invoice.vwSummary
AS
	WITH Projects AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 1 THEN 0 ELSE CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 3 THEN 2 ELSE Invoice.tbInvoice.InvoiceTypeCode END END AS InvoiceTypeCode, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.InvoiceValue * - 1 ELSE Invoice.tbProject.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.TaxValue * - 1 ELSE Invoice.tbProject.TaxValue END AS TaxValue
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoicedOn >= (	
						SELECT MIN( App.tbYearPeriod.StartOn) FROM App.tbYear 
						INNER JOIN App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber 
						WHERE ( App.tbYear.CashStatusCode < 3)))
	), items AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 1 THEN 0 ELSE CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 3 THEN 2 ELSE Invoice.tbInvoice.InvoiceTypeCode END END AS InvoiceTypeCode, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.TaxValue * - 1 ELSE Invoice.tbItem.TaxValue END AS TaxValue
		FROM            Invoice.tbItem INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoicedOn >= (	
						SELECT MIN( App.tbYearPeriod.StartOn) FROM App.tbYear 
						INNER JOIN App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber 
						WHERE ( App.tbYear.CashStatusCode < 3)))
	), invoice_entries AS
	(
		SELECT     StartOn, InvoiceTypeCode, InvoiceValue, TaxValue
		FROM         items
		UNION
		SELECT     StartOn, InvoiceTypeCode, InvoiceValue, TaxValue
		FROM         Projects
	), invoice_totals AS
	(
		SELECT     invoice_entries.StartOn, invoice_entries.InvoiceTypeCode, Invoice.tbType.InvoiceType, 
							  SUM(invoice_entries.InvoiceValue) AS TotalInvoiceValue, SUM(invoice_entries.TaxValue) AS TotalTaxValue
		FROM         invoice_entries INNER JOIN
							  Invoice.tbType ON invoice_entries.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		GROUP BY invoice_entries.StartOn, invoice_entries.InvoiceTypeCode, Invoice.tbType.InvoiceType
	), invoice_margin AS
	(
		SELECT     StartOn, 4 AS InvoiceTypeCode, (SELECT CAST(Message AS NVARCHAR(10)) FROM App.tbText WHERE TextId = 3004) AS InvoiceType, SUM(TotalInvoiceValue) AS TotalInvoiceValue, SUM(TotalTaxValue) 
							  AS TotalTaxValue
		FROM         invoice_totals
		GROUP BY StartOn
	)
	SELECT     CONCAT(DATENAME(yyyy, StartOn), '/', FORMAT(MONTH(StartOn), '00')) AS PeriodOn, StartOn, InvoiceTypeCode, InvoiceType AS InvoiceType, 
						  ABS(TotalInvoiceValue) AS TotalInvoiceValue, ABS(TotalTaxValue) AS TotalTaxValue
	FROM         invoice_totals
	UNION
	SELECT     CONCAT(DATENAME(yyyy, StartOn), '/', FORMAT(MONTH(StartOn), '00')) AS PeriodOn, StartOn, InvoiceTypeCode, InvoiceType AS InvoiceType, 
						  TotalInvoiceValue, TotalTaxValue
	FROM         invoice_margin;

