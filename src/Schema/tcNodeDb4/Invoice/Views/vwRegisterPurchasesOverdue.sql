CREATE VIEW Invoice.vwRegisterPurchasesOverdue
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
                         Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) 
                         ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, Invoice.tbMirror.PaymentAddress, 
                         Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
                         Invoice.tbMirrorReference ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbMirrorReference.InvoiceNumber LEFT OUTER JOIN
                         Invoice.tbMirror ON Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress
WHERE        (Invoice.tbInvoice.InvoiceTypeCode > 1) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
