CREATE VIEW Cash.vwSummary
AS
	WITH company AS
	(
		SELECT 0 AS SummaryId, SUM( Org.tbAccount.CurrentBalance) AS CompanyBalance 
		FROM Org.tbAccount WHERE ( Org.tbAccount.AccountClosed = 0) AND (Org.tbAccount.AccountTypeCode = 0)
	), corp_tax_invoiced AS
	(
		SELECT TOP (1)  0 AS SummaryId, Balance AS CorpTaxBalance 
		FROM Cash.vwTaxCorpStatement 
		ORDER BY StartOn DESC
	), corp_tax_ordered AS
	(
		SELECT 0 AS SummaryId, SUM(TaxDue) AS CorpTaxBalance
		FROM Cash.vwTaxCorpAccruals
	), vat_invoiced AS
	(
		SELECT TOP (1)  0 AS SummaryId, Balance AS VatBalance 
		FROM Cash.vwTaxVatStatement 
		ORDER BY StartOn DESC, VatDue DESC
	), vat_accruals AS
	(
		SELECT 0 AS SummaryId, SUM(VatDue) AS VatBalance
		FROM Cash.vwTaxVatAccruals
	), invoices AS
	(
		SELECT     Invoice.tbInvoice.InvoiceNumber, CASE Invoice.tbInvoice.InvoiceTypeCode WHEN 0 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) 
						  WHEN 3 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) ELSE 0 END AS ToCollect, 
						  CASE Invoice.tbInvoice.InvoiceTypeCode WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) WHEN 2 THEN (InvoiceValue + TaxValue) 
						  - (PaidValue + PaidTaxValue) ELSE 0 END AS ToPay, CASE Invoice.tbType.CashModeCode WHEN 0 THEN (TaxValue - PaidTaxValue) 
						  * - 1 WHEN 1 THEN TaxValue - PaidTaxValue END AS TaxValue
		FROM         Invoice.tbInvoice INNER JOIN
							  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE     (Invoice.tbInvoice.InvoiceStatusCode = 1) OR
						  (Invoice.tbInvoice.InvoiceStatusCode = 2)
	), invoice_totals AS
	(
		SELECT 0 AS SummaryId, ISNULL(SUM(ToCollect), 0) AS Collect, ISNULL(SUM(ToPay), 0) AS Pay, ISNULL(SUM(TaxValue), 0) AS TaxValue
		FROM  invoices
	), summary_base AS
	(
		SELECT Collect, Pay, TaxValue + vat_invoiced.VatBalance + vat_accruals.VatBalance + corp_tax_invoiced.CorpTaxBalance + corp_tax_ordered.CorpTaxBalance AS Tax, CompanyBalance
		FROM company 
				JOIN corp_tax_invoiced ON company.SummaryId = corp_tax_invoiced.SummaryId
				JOIN corp_tax_ordered ON company.SummaryId = corp_tax_ordered.SummaryId
				JOIN vat_invoiced ON company.SummaryId = vat_invoiced.SummaryId
				JOIN vat_accruals ON company.SummaryId = vat_accruals.SummaryId
				JOIN invoice_totals ON company.SummaryId = invoice_totals.SummaryId
	)
	SELECT CURRENT_TIMESTAMP AS Timestamp, Collect, Pay, Tax, CompanyBalance AS Cash, CompanyBalance + Collect - Pay - Tax AS Balance
	FROM    summary_base;
