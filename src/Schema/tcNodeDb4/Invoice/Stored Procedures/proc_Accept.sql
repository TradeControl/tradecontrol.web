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
	          FROM         Invoice.tbProject
	          WHERE     (InvoiceNumber = @InvoiceNumber))
		BEGIN
			BEGIN TRANSACTION
			
			EXEC Invoice.proc_Total @InvoiceNumber
			
			UPDATE    Invoice.tbInvoice
			SET              InvoiceStatusCode = 1
			WHERE     (InvoiceNumber = @InvoiceNumber) AND (InvoiceStatusCode = 0); 
	
			WITH Project_codes AS
			(
				SELECT ProjectCode
				FROM Invoice.tbProject 
				WHERE InvoiceNumber = @InvoiceNumber
				GROUP BY ProjectCode
			), deliveries AS
			(
				SELECT invoices.ProjectCode, SUM(Quantity) QuantityDelivered
				FROM Invoice.tbProject invoices JOIN Project_codes ON invoices.ProjectCode = Project_codes.ProjectCode
				GROUP BY invoices.ProjectCode
			)
			UPDATE Project
			SET ProjectStatusCode = 3
			FROM Project.tbProject Project JOIN deliveries ON Project.ProjectCode = deliveries.ProjectCode
			WHERE Quantity <= QuantityDelivered;
			
			COMMIT TRANSACTION
		END
			
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
