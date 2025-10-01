CREATE   FUNCTION Invoice.fnEditDebitCandidates (@InvoiceNumber nvarchar(20), @AccountCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditTasks AS 
		(
			SELECT        TaskCode
			FROM            Invoice.tbTask
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT tbInvoiceTask.TaskCode, tbInvoiceTask.InvoiceNumber, tbTask.ActivityCode, Invoice.tbStatus.InvoiceStatus, Usr.tbUser.UserName, Invoice.tbInvoice.InvoicedOn, tbInvoiceTask.InvoiceValue, 
								tbTask.TaskTitle
		FROM            Usr.tbUser INNER JOIN
								Invoice.tbInvoice INNER JOIN
								Invoice.tbTask AS tbInvoiceTask ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceTask.InvoiceNumber INNER JOIN
								Task.tbTask ON tbInvoiceTask.TaskCode = tbTask.TaskCode INNER JOIN
								Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode ON Usr.tbUser.UserId = Invoice.tbInvoice.UserId LEFT OUTER JOIN
								InvoiceEditTasks  ON tbTask.TaskCode = InvoiceEditTasks.TaskCode
		WHERE        (Invoice.tbInvoice.AccountCode = @AccountCode) AND (Invoice.tbInvoice.InvoiceTypeCode = 2) AND (InvoiceEditTasks.TaskCode IS NULL)
		ORDER BY Invoice.tbInvoice.InvoicedOn DESC
	);
