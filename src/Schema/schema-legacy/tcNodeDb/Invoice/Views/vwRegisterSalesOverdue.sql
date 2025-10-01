CREATE VIEW Invoice.vwRegisterSalesOverdue
AS
	SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.AccountCode, Org.tbOrg.AccountName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
							 Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
							 CASE CashModeCode WHEN 1 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
							 CASE CashModeCode WHEN 1 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, CASE CashModeCode WHEN 1 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) 
							 - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, 
							 Cash.tbChangeReference.PaymentAddress, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							 Cash.tbChangeReference ON Invoice.tbInvoice.InvoiceNumber = Cash.tbChangeReference.InvoiceNumber
	WHERE        (Invoice.tbInvoice.InvoiceTypeCode < 2) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
