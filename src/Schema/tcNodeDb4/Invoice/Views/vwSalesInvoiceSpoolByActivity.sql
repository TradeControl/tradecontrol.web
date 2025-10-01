CREATE VIEW Invoice.vwSalesInvoiceSpoolByObject
AS
WITH invoice AS 
(
	SELECT        sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, 
							Subject.tbSubject.EmailAddress, Subject.tbSubject.AddressCode, Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, MIN(Project.tbProject.ActionedOn) AS FirstActionedOn, 
							SUM(tbInvoiceProject.Quantity) AS ObjectQuantity, tbInvoiceProject.TaxCode, SUM(tbInvoiceProject.InvoiceValue) AS ObjectInvoiceValue, SUM(tbInvoiceProject.TaxValue) AS ObjectTaxValue
	FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
							Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							Subject.tbSubject ON sales_invoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId INNER JOIN
							Invoice.tbProject AS tbInvoiceProject ON sales_invoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
							Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
							Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	WHERE        EXISTS
								(SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
									FROM            App.tbDocSpool AS doc
									WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))
	GROUP BY sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue, sales_invoice.TaxValue, sales_invoice.PaymentTerms, Subject.tbSubject.EmailAddress, Subject.tbSubject.AddressCode, 
							Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode
)
SELECT        invoice_1.InvoiceNumber, invoice_1.InvoiceType, invoice_1.InvoiceStatusCode, invoice_1.UserName, invoice_1.SubjectCode, invoice_1.SubjectName, invoice_1.InvoiceStatus, invoice_1.InvoicedOn, 
                        Invoice.tbInvoice.Notes, Subject.tbAddress.Address AS InvoiceAddress, invoice_1.InvoiceValueTotal, invoice_1.TaxValueTotal, invoice_1.PaymentTerms, invoice_1.EmailAddress, invoice_1.AddressCode, 
                        invoice_1.ObjectCode, invoice_1.UnitOfMeasure, invoice_1.FirstActionedOn, invoice_1.ObjectQuantity, invoice_1.TaxCode, invoice_1.ObjectInvoiceValue, invoice_1.ObjectTaxValue
FROM            invoice AS invoice_1 INNER JOIN
                        Invoice.tbInvoice ON invoice_1.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber LEFT OUTER JOIN
                        Subject.tbAddress ON invoice_1.AddressCode = Subject.tbAddress.AddressCode;

