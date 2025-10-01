CREATE VIEW Invoice.vwSalesInvoiceSpool
AS
SELECT        sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, sales_invoice.DueOn, sales_invoice.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ObjectCode, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
                         Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON sales_invoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON sales_invoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE        (sales_invoice.InvoiceTypeCode = 0) AND EXISTS
                             (SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn, RowVer
                               FROM            App.tbDocSpool AS doc
                               WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))

