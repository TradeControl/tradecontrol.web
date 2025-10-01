CREATE   FUNCTION Invoice.fnEditCreditCandidates (@InvoiceNumber nvarchar(20), @SubjectCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditProjects AS 
		(
			SELECT        ProjectCode
			FROM            Invoice.tbProject
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT tbInvoiceProject.ProjectCode, tbInvoiceProject.InvoiceNumber, tbProject.ObjectCode, Invoice.tbStatus.InvoiceStatus, Usr.tbUser.UserName, Invoice.tbInvoice.InvoicedOn, tbInvoiceProject.InvoiceValue, 
								tbProject.ProjectTitle
		FROM            Usr.tbUser INNER JOIN
								Invoice.tbInvoice INNER JOIN
								Invoice.tbProject AS tbInvoiceProject ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
								Project.tbProject AS tbProject ON tbInvoiceProject.ProjectCode = tbProject.ProjectCode INNER JOIN
								Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode ON Usr.tbUser.UserId = Invoice.tbInvoice.UserId LEFT OUTER JOIN
								InvoiceEditProjects AS InvoiceEditProjects ON tbProject.ProjectCode = InvoiceEditProjects.ProjectCode
		WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceTypeCode = 0) AND (InvoiceEditProjects.ProjectCode IS NULL)
		ORDER BY Invoice.tbInvoice.InvoicedOn DESC
	);
