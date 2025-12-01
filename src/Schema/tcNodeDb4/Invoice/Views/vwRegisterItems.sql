CREATE VIEW Invoice.vwRegisterItems
AS
	SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
					Invoice.tbInvoice.InvoiceNumber, Invoice.tbItem.CashCode AS ProjectCode, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 Invoice.tbItem.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn,
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.TaxValue * - 1 ELSE Invoice.tbItem.TaxValue END AS TaxValue, 
							 CAST(Invoice.tbItem.ItemReference as nvarchar(100)) ItemReference, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber INNER JOIN
							 Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode;
