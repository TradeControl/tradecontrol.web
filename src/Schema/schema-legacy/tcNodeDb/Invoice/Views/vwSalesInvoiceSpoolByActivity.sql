CREATE VIEW Invoice.vwSalesInvoiceSpoolByActivity
AS
WITH invoice AS 
(
	SELECT        sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.AccountCode, Org.tbOrg.AccountName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, 
							Org.tbOrg.EmailAddress, Org.tbOrg.AddressCode, Activity.tbActivity.ActivityCode, Activity.tbActivity.UnitOfMeasure, MIN(Task.tbTask.ActionedOn) AS FirstActionedOn, 
							SUM(tbInvoiceTask.Quantity) AS ActivityQuantity, tbInvoiceTask.TaxCode, SUM(tbInvoiceTask.InvoiceValue) AS ActivityInvoiceValue, SUM(tbInvoiceTask.TaxValue) AS ActivityTaxValue
	FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
							Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							Org.tbOrg ON sales_invoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId INNER JOIN
							Invoice.tbTask AS tbInvoiceTask ON sales_invoice.InvoiceNumber = tbInvoiceTask.InvoiceNumber INNER JOIN
							Task.tbTask ON tbInvoiceTask.TaskCode = Task.tbTask.TaskCode INNER JOIN
							Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode INNER JOIN
							Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	WHERE        EXISTS
								(SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
									FROM            App.tbDocSpool AS doc
									WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))
	GROUP BY sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.AccountCode, Org.tbOrg.AccountName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue, sales_invoice.TaxValue, sales_invoice.PaymentTerms, Org.tbOrg.EmailAddress, Org.tbOrg.AddressCode, 
							Activity.tbActivity.ActivityCode, Activity.tbActivity.UnitOfMeasure, tbInvoiceTask.TaxCode
)
SELECT        invoice_1.InvoiceNumber, invoice_1.InvoiceType, invoice_1.InvoiceStatusCode, invoice_1.UserName, invoice_1.AccountCode, invoice_1.AccountName, invoice_1.InvoiceStatus, invoice_1.InvoicedOn, 
                        Invoice.tbInvoice.Notes, Org.tbAddress.Address AS InvoiceAddress, invoice_1.InvoiceValueTotal, invoice_1.TaxValueTotal, invoice_1.PaymentTerms, invoice_1.EmailAddress, invoice_1.AddressCode, 
                        invoice_1.ActivityCode, invoice_1.UnitOfMeasure, invoice_1.FirstActionedOn, invoice_1.ActivityQuantity, invoice_1.TaxCode, invoice_1.ActivityInvoiceValue, invoice_1.ActivityTaxValue
FROM            invoice AS invoice_1 INNER JOIN
                        Invoice.tbInvoice ON invoice_1.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber LEFT OUTER JOIN
                        Org.tbAddress ON invoice_1.AddressCode = Org.tbAddress.AddressCode;

