CREATE VIEW Invoice.vwRegisterProjects
AS
	SELECT (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
			Invoice.tbInvoice.InvoiceNumber, InvoiceProject.ProjectCode, Project.ProjectTitle, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 InvoiceProject.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn,  Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, InvoiceProject.Quantity,
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN InvoiceProject.InvoiceValue * - 1 ELSE InvoiceProject.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN InvoiceProject.TaxValue * - 1 ELSE InvoiceProject.TaxValue END AS TaxValue, 
							 Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbProject AS InvoiceProject ON Invoice.tbInvoice.InvoiceNumber = InvoiceProject.InvoiceNumber INNER JOIN
							 Cash.tbCode ON InvoiceProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Project.tbProject AS Project ON InvoiceProject.ProjectCode = Project.ProjectCode AND InvoiceProject.ProjectCode = Project.ProjectCode LEFT OUTER JOIN
							 App.tbTaxCode ON InvoiceProject.TaxCode = App.tbTaxCode.TaxCode;
