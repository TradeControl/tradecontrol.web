CREATE   PROCEDURE Invoice.proc_CancelById(@UserId nvarchar(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE       Project
		SET                ProjectStatusCode = 2
		FROM            Project.tbProject AS Project INNER JOIN
								 Invoice.tbProject AS InvoiceProject ON Project.ProjectCode = InvoiceProject.ProjectCode AND Project.ProjectCode = InvoiceProject.ProjectCode INNER JOIN
								 Invoice.tbInvoice ON InvoiceProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0 OR Invoice.tbInvoice.InvoiceTypeCode = 2) 
			AND (Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Project.ProjectStatusCode = 3) AND (Invoice.tbInvoice.UserId = @UserId)
	                      
		DELETE Invoice.tbInvoice
		FROM         Invoice.tbInvoice INNER JOIN
							  Usr.vwCredentials ON Invoice.tbInvoice.UserId = Usr.vwCredentials.UserId
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Invoice.tbInvoice.UserId = @UserId)
		
		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
