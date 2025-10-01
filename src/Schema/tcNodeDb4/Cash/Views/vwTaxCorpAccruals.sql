CREATE VIEW Cash.vwTaxCorpAccruals
AS
	WITH corptax_ordered_confirmed AS
	(
		SELECT        Project.ProjectCode, Project.ActionOn, Project.Quantity, CASE WHEN Cash.tbCategory.CashPolarityCode = 0 THEN Project.TotalCharge * - 1 ELSE Project.TotalCharge END AS TotalCharge
		FROM            Project.tbProject AS Project INNER JOIN
								 Cash.tbCode ON Project.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE        (Project.ProjectStatusCode BETWEEN 1 AND 2) AND (Project.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) AS HorizonOn FROM App.tbOptions))
	), corptax_ordered_invoices AS
	(
		SELECT corptax_ordered_confirmed.ProjectCode, Project_invoice.Quantity,
			CASE WHEN invoice_type.CashPolarityCode = 0 THEN Project_invoice.InvoiceValue * -1 ELSE Project_invoice.InvoiceValue END AS InvoiceValue
		FROM corptax_ordered_confirmed JOIN Invoice.tbProject Project_invoice ON corptax_ordered_confirmed.ProjectCode = Project_invoice.ProjectCode
			JOIN Invoice.tbInvoice invoice ON Project_invoice.InvoiceNumber = invoice.InvoiceNumber
			JOIN Invoice.tbType invoice_type ON invoice_type.InvoiceTypeCode = invoice.InvoiceTypeCode
	), corptax_ordered AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= corptax_ordered_confirmed.ActionOn) ORDER BY StartOn DESC) AS StartOn, 
			corptax_ordered_confirmed.ProjectCode,
			corptax_ordered_confirmed.Quantity - ISNULL(corptax_ordered_invoices.Quantity, 0) AS QuantityRemaining,
			corptax_ordered_confirmed.TotalCharge - ISNULL(corptax_ordered_invoices.InvoiceValue, 0) AS OrderValue
		FROM corptax_ordered_confirmed 
			LEFT JOIN corptax_ordered_invoices ON corptax_ordered_confirmed.ProjectCode = corptax_ordered_invoices.ProjectCode
	)
	SELECT corptax_ordered.StartOn, ProjectCode, QuantityRemaining, OrderValue, OrderValue * CorporationTaxRate AS TaxDue
	FROM corptax_ordered JOIN App.tbYearPeriod year_period ON corptax_ordered.StartOn = year_period.StartOn;
