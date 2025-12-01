CREATE VIEW Invoice.vwAgedDebtSales
AS
SELECT TOP 100 PERCENT  Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
                         Invoice.tbType.InvoiceType, (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) AS UnpaidValue, DATEDIFF(DD, CURRENT_TIMESTAMP, 
                         Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, 
                         Invoice.tbInvoice.Notes
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode
WHERE        (Invoice.tbInvoice.InvoiceTypeCode < 2) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
ORDER BY Invoice.tbInvoice.ExpectedOn;

