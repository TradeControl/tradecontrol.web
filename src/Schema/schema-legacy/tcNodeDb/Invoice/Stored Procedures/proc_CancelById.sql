CREATE   PROCEDURE Invoice.proc_CancelById(@UserId nvarchar(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE       Task
		SET                TaskStatusCode = 2
		FROM            Task.tbTask AS Task INNER JOIN
								 Invoice.tbTask AS InvoiceTask ON Task.TaskCode = InvoiceTask.TaskCode AND Task.TaskCode = InvoiceTask.TaskCode INNER JOIN
								 Invoice.tbInvoice ON InvoiceTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0 OR Invoice.tbInvoice.InvoiceTypeCode = 2) 
			AND (Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Task.TaskStatusCode = 3) AND (Invoice.tbInvoice.UserId = @UserId)
	                      
		DELETE Invoice.tbInvoice
		FROM         Invoice.tbInvoice INNER JOIN
							  Usr.vwCredentials ON Invoice.tbInvoice.UserId = Usr.vwCredentials.UserId
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Invoice.tbInvoice.UserId = @UserId)
		
		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
