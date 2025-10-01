CREATE VIEW Cash.vwTaxVatAccruals
AS
	WITH task_invoiced_quantity AS
	(
		SELECT        Invoice.tbTask.TaskCode, SUM(Invoice.tbTask.Quantity) AS InvoiceQuantity
		FROM            Invoice.tbTask INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0) OR
								 (Invoice.tbInvoice.InvoiceTypeCode = 2)
		GROUP BY Invoice.tbTask.TaskCode
	), task_transactions AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Task.tbTask.ActionOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Task.tbTask.TaskCode, Task.tbTask.TaxCode,
				Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 0) AS QuantityRemaining,
				Task.tbTask.UnitCharge * (Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 0)) AS TotalValue, 
				Task.tbTask.UnitCharge * (Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 0)) * App.tbTaxCode.TaxRate AS TaxValue,
				App.tbTaxCode.TaxRate,
				Org.tbOrg.EUJurisdiction,
				Cash.tbCategory.CashModeCode
		FROM    Task.tbTask INNER JOIN
				Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode INNER JOIN
				Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				App.tbTaxCode ON Task.tbTask.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
				task_invoiced_quantity ON Task.tbTask.TaskCode = task_invoiced_quantity.TaskCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Task.tbTask.TaskStatusCode > 0) AND (Task.tbTask.TaskStatusCode < 3) AND (App.tbTaxCode.TaxTypeCode = 1)
			AND (Task.tbTask.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions))
	), task_dataset AS
	(
		SELECT StartOn, TaskCode, TaxCode, QuantityRemaining, TotalValue, TaxValue, TaxRate,
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashModeCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomeSales, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashModeCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomePurchases, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashModeCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportSales, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashModeCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportPurchases, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashModeCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomeSalesVat, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashModeCode WHEN 0 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomePurchasesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashModeCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS ExportSalesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashModeCode WHEN 0 THEN TaxValue ELSE 0 END)  ELSE 0 END as float) AS ExportPurchasesVat
		FROM task_transactions
	)
	SELECT task_dataset.*,
		 (HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM task_dataset
		JOIN App.tbYearPeriod AS year_period ON task_dataset.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber;

