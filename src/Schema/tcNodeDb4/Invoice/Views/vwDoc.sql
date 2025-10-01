CREATE VIEW Invoice.vwDoc
AS
	SELECT     Subject.tbSubject.EmailAddress, Usr.tbUser.UserName, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
						  Invoice.tbInvoice.InvoiceNumber, Invoice.tbType.InvoiceType, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, 
						  Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, 
						  Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue AS TotalValue, 
						  Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM         Invoice.tbInvoice INNER JOIN
						  Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
						  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
						  Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
						  Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode LEFT OUTER JOIN
						  Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode
