CREATE VIEW Cash.vwTaxVatAccruals
AS
	WITH Project_invoiced_quantity AS
	(
		SELECT        Invoice.tbProject.ProjectCode, SUM(Invoice.tbProject.Quantity) AS InvoiceQuantity
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0) OR
								 (Invoice.tbInvoice.InvoiceTypeCode = 2)
		GROUP BY Invoice.tbProject.ProjectCode
	), Project_transactions AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Project.tbProject.ActionOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Project.tbProject.ProjectCode, Project.tbProject.TaxCode,
				Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0) AS QuantityRemaining,
				Project.tbProject.UnitCharge * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0)) AS TotalValue, 
				Project.tbProject.UnitCharge * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0)) * App.tbTaxCode.TaxRate AS TaxValue,
				App.tbTaxCode.TaxRate,
				Subject.tbSubject.EUJurisdiction,
				Cash.tbCategory.CashPolarityCode
		FROM    Project.tbProject INNER JOIN
				Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
				Project_invoiced_quantity ON Project.tbProject.ProjectCode = Project_invoiced_quantity.ProjectCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Project.tbProject.ProjectStatusCode > 0) AND (Project.tbProject.ProjectStatusCode < 3) AND (App.tbTaxCode.TaxTypeCode = 1)
			AND (Project.tbProject.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions))
	), Project_dataset AS
	(
		SELECT StartOn, ProjectCode, TaxCode, QuantityRemaining, TotalValue, TaxValue, TaxRate,
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomeSales, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomePurchases, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportSales, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportPurchases, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomeSalesVat, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomePurchasesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS ExportSalesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END)  ELSE 0 END as float) AS ExportPurchasesVat
		FROM Project_transactions
	)
	SELECT Project_dataset.*,
		 (HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM Project_dataset
		JOIN App.tbYearPeriod AS year_period ON Project_dataset.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber;

