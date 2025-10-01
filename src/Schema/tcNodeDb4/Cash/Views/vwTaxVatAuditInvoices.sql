CREATE VIEW Cash.vwTaxVatAuditInvoices
AS
	WITH vat_transactions AS
	(
		SELECT   Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbItem.TaxCode, Invoice.tbItem.InvoiceValue, Invoice.tbItem.TaxValue,
								  ROUND((Invoice.tbItem.TaxValue /  Invoice.tbItem.InvoiceValue), 3) As CalcRate,
								 App.tbTaxCode.TaxRate, Subject.tbSubject.EUJurisdiction, Invoice.tbItem.CashCode AS IdentityCode, Cash.tbCode.CashDescription As ItemDescription
		FROM            Invoice.tbItem INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								 Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Invoice.tbItem.InvoiceValue <> 0)
		UNION
		SELECT   Invoice.tbInvoice.InvoicedOn, Invoice.tbProject.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbProject.TaxCode, Invoice.tbProject.InvoiceValue, Invoice.tbProject.TaxValue, 
								 ROUND(Invoice.tbProject.TaxValue / Invoice.tbProject.InvoiceValue, 3) AS CalcRate, App.tbTaxCode.TaxRate, Subject.tbSubject.EUJurisdiction, Invoice.tbProject.ProjectCode AS IdentityCode, tbProject_1.ProjectTitle As ItemDescription
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								 Project.tbProject AS tbProject_1 ON Invoice.tbProject.ProjectCode = tbProject_1.ProjectCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Invoice.tbProject.InvoiceValue <> 0)
	)
	, vat_dataset AS
	(
		SELECT  (SELECT PayTo FROM Cash.fnTaxTypeDueDates(1) due_dates WHERE vat_transactions.InvoicedOn >= PayFrom AND vat_transactions.InvoicedOn < PayTo) AS StartOn,
		 vat_transactions.InvoicedOn, InvoiceNumber, invoice_type.InvoiceType, vat_transactions.InvoiceTypeCode, TaxCode, InvoiceValue, TaxValue, TaxRate, EUJurisdiction, IdentityCode, ItemDescription,
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomeSales, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomePurchases, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportSales, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportPurchases, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS HomeSalesVat, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS HomePurchasesVat, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS ExportSalesVat, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS ExportPurchasesVat
		FROM  vat_transactions 
			JOIN Invoice.tbType invoice_type ON vat_transactions.InvoiceTypeCode = invoice_type.InvoiceTypeCode
	)
	SELECT CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, vat_dataset.*,
		 (HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM vat_dataset
		JOIN App.tbYearPeriod AS year_period ON vat_dataset.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber;
