CREATE VIEW Cash.vwTaxCorpAccruals
AS
	WITH corptax_ordered_confirmed AS
	(
		SELECT        task.TaskCode, task.ActionOn, task.Quantity, CASE WHEN Cash.tbCategory.CashModeCode = 0 THEN task.TotalCharge * - 1 ELSE task.TotalCharge END AS TotalCharge
		FROM            Task.tbTask AS task INNER JOIN
								 Cash.tbCode ON task.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE        (task.TaskStatusCode BETWEEN 1 AND 2) AND (task.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) AS HorizonOn FROM App.tbOptions))
	), corptax_ordered_invoices AS
	(
		SELECT corptax_ordered_confirmed.TaskCode, task_invoice.Quantity,
			CASE WHEN invoice_type.CashModeCode = 0 THEN task_invoice.InvoiceValue * -1 ELSE task_invoice.InvoiceValue END AS InvoiceValue
		FROM corptax_ordered_confirmed JOIN Invoice.tbTask task_invoice ON corptax_ordered_confirmed.TaskCode = task_invoice.TaskCode
			JOIN Invoice.tbInvoice invoice ON task_invoice.InvoiceNumber = invoice.InvoiceNumber
			JOIN Invoice.tbType invoice_type ON invoice_type.InvoiceTypeCode = invoice.InvoiceTypeCode
	), corptax_ordered AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= corptax_ordered_confirmed.ActionOn) ORDER BY StartOn DESC) AS StartOn, 
			corptax_ordered_confirmed.TaskCode,
			corptax_ordered_confirmed.Quantity - ISNULL(corptax_ordered_invoices.Quantity, 0) AS QuantityRemaining,
			corptax_ordered_confirmed.TotalCharge - ISNULL(corptax_ordered_invoices.InvoiceValue, 0) AS OrderValue
		FROM corptax_ordered_confirmed 
			LEFT JOIN corptax_ordered_invoices ON corptax_ordered_confirmed.TaskCode = corptax_ordered_invoices.TaskCode
	)
	SELECT corptax_ordered.StartOn, TaskCode, QuantityRemaining, OrderValue, OrderValue * CorporationTaxRate AS TaxDue
	FROM corptax_ordered JOIN App.tbYearPeriod year_period ON corptax_ordered.StartOn = year_period.StartOn;
