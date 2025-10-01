CREATE   VIEW Subject.vwInvoiceProjects
AS
	SELECT        Invoice.tbInvoice.SubjectCode, tbInvoiceProject.InvoiceNumber, tbInvoiceProject.ProjectCode, Project.tbProject.ContactName, Invoice.tbInvoice.InvoicedOn, tbInvoiceProject.Quantity, tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue, 
							 tbInvoiceProject.CashCode, tbInvoiceProject.TaxCode, Invoice.tbStatus.InvoiceStatus, Project.tbProject.ProjectNotes, Cash.tbCode.CashDescription, Invoice.tbInvoice.InvoiceStatusCode, Project.tbProject.ProjectTitle, Subject.tbSubject.SubjectName, 
							 Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashPolarityCode, Invoice.tbInvoice.PaidTaxValue, Invoice.tbInvoice.PaidValue
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbProject AS tbInvoiceProject ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
							 Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
							 Cash.tbCode ON tbInvoiceProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	WHERE        (Invoice.tbInvoice.InvoiceStatusCode > 0);

