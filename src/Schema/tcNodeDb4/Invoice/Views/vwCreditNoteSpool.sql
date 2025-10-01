CREATE VIEW Invoice.vwCreditNoteSpool
AS
SELECT        credit_note.InvoiceNumber, credit_note.Printed, Invoice.tbType.InvoiceType, credit_note.InvoiceStatusCode, Usr.tbUser.UserName, credit_note.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         credit_note.InvoicedOn, credit_note.InvoiceValue AS InvoiceValueTotal, credit_note.TaxValue AS TaxValueTotal, credit_note.PaymentTerms, credit_note.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS credit_note INNER JOIN
                         Invoice.tbStatus ON credit_note.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON credit_note.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON credit_note.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON credit_note.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON credit_note.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE credit_note.InvoiceTypeCode = 1 
	AND EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 5 AND UserName = SUSER_SNAME() AND credit_note.InvoiceNumber = doc.DocumentNumber);

