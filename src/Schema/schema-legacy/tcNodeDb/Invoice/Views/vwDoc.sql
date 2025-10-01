CREATE VIEW Invoice.vwDoc
AS
	SELECT     Org.tbOrg.EmailAddress, Usr.tbUser.UserName, Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbAddress.Address AS InvoiceAddress, 
						  Invoice.tbInvoice.InvoiceNumber, Invoice.tbType.InvoiceType, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, 
						  Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, 
						  Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue AS TotalValue, 
						  Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM         Invoice.tbInvoice INNER JOIN
						  Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
						  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
						  Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
						  Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode LEFT OUTER JOIN
						  Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode
