CREATE VIEW App.vwDocSalesInvoice
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.Printed, Invoice.tbInvoice.Spooled, Invoice.tbInvoice.InvoiceStatusCode, Usr.tbUser.UserName, Invoice.tbInvoice.SubjectCode, 
                         Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
                         Subject.tbSubject.EmailAddress, Invoice.tbInvoice.RowVer
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0);
