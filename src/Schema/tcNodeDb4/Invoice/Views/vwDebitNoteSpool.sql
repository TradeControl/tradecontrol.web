CREATE VIEW Invoice.vwDebitNoteSpool
AS
SELECT        debit_note.Printed, debit_note.InvoiceNumber, Invoice.tbType.InvoiceType, debit_note.InvoiceStatusCode, Usr.tbUser.UserName, debit_note.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         debit_note.InvoicedOn, debit_note.InvoiceValue AS InvoiceValueTotal, debit_note.TaxValue AS TaxValueTotal, debit_note.PaymentTerms, debit_note.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS debit_note INNER JOIN
                         Invoice.tbStatus ON debit_note.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON debit_note.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON debit_note.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON debit_note.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON debit_note.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE debit_note.InvoiceTypeCode = 3 AND
	EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 6 AND UserName = SUSER_SNAME() AND debit_note.InvoiceNumber = doc.DocumentNumber);

