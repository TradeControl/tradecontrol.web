CREATE PROCEDURE Invoice.proc_Accept 
	(
	@InvoiceNumber nvarchar(20)
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     InvoiceNumber
	          FROM         Invoice.tbItem
	          WHERE     (InvoiceNumber = @InvoiceNumber)) 
		or EXISTS(SELECT     InvoiceNumber
	          FROM         Invoice.tbTask
	          WHERE     (InvoiceNumber = @InvoiceNumber))
		BEGIN
			BEGIN TRANSACTION
			
			EXEC Invoice.proc_Total @InvoiceNumber
			
			UPDATE    Invoice.tbInvoice
			SET              InvoiceStatusCode = 1
			WHERE     (InvoiceNumber = @InvoiceNumber) AND (InvoiceStatusCode = 0); 
	
			WITH task_codes AS
			(
				SELECT TaskCode
				FROM Invoice.tbTask 
				WHERE InvoiceNumber = @InvoiceNumber
				GROUP BY TaskCode
			), deliveries AS
			(
				SELECT invoices.TaskCode, SUM(Quantity) QuantityDelivered
				FROM Invoice.tbTask invoices JOIN task_codes ON invoices.TaskCode = task_codes.TaskCode
				GROUP BY invoices.TaskCode
			)
			UPDATE task
			SET TaskStatusCode = 3
			FROM Task.tbTask task JOIN deliveries ON task.TaskCode = deliveries.TaskCode
			WHERE Quantity <= QuantityDelivered;
			
			COMMIT TRANSACTION
		END
			
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
