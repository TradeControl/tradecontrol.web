
CREATE   PROCEDURE Invoice.proc_AddTask 
	(
	@InvoiceNumber nvarchar(20),
	@TaskCode nvarchar(20)	
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@InvoiceTypeCode smallint
		, @InvoiceQuantity float
		, @QuantityInvoiced float

		IF EXISTS(SELECT     InvoiceNumber, TaskCode
				  FROM         Invoice.tbTask
				  WHERE     (InvoiceNumber = @InvoiceNumber) AND (TaskCode = @TaskCode))
			RETURN
		
		SELECT   @InvoiceTypeCode = InvoiceTypeCode
		FROM         Invoice.tbInvoice
		WHERE     (InvoiceNumber = @InvoiceNumber) 

		IF EXISTS(SELECT     SUM( Invoice.tbTask.Quantity) AS QuantityInvoiced
				  FROM         Invoice.tbTask INNER JOIN
										Invoice.tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
										Invoice.tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbTask.TaskCode = @TaskCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
			BEGIN
			SELECT TOP 1 @QuantityInvoiced = isnull(SUM( Invoice.tbTask.Quantity), 0)
			FROM         Invoice.tbTask INNER JOIN
					tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
					tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbTask.TaskCode = @TaskCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)				
			END
		ELSE
			SET @QuantityInvoiced = 0
		
		IF @InvoiceTypeCode = 1 or @InvoiceTypeCode = 3
			BEGIN
			IF EXISTS(SELECT     SUM( Invoice.tbTask.Quantity) AS QuantityInvoiced
					  FROM         Invoice.tbTask INNER JOIN
											tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
					  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
											tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbTask.TaskCode = @TaskCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
				BEGIN
				SELECT TOP 1 @InvoiceQuantity = isnull(@QuantityInvoiced, 0) - isnull(SUM( Invoice.tbTask.Quantity), 0)
				FROM         Invoice.tbTask INNER JOIN
						tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
						tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbTask.TaskCode = @TaskCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)										
				END
			ELSE
				SET @InvoiceQuantity = isnull(@QuantityInvoiced, 0)
			END
		ELSE
			BEGIN
			SELECT  @InvoiceQuantity = Quantity - isnull(@QuantityInvoiced, 0)
			FROM         Task.tbTask
			WHERE     (TaskCode = @TaskCode)
			END
			
		IF isnull(@InvoiceQuantity, 0) <= 0
			SET @InvoiceQuantity = 1
		
		INSERT INTO Invoice.tbTask
							  (InvoiceNumber, TaskCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT     @InvoiceNumber AS InvoiceNumber, TaskCode, @InvoiceQuantity AS Quantity, UnitCharge * @InvoiceQuantity AS InvoiceValue, CashCode, 
							  TaxCode
		FROM         Task.tbTask
		WHERE     (TaskCode = @TaskCode)

		UPDATE Task.tbTask
		SET ActionedOn = CURRENT_TIMESTAMP
		WHERE TaskCode = @TaskCode;
	
		EXEC Invoice.proc_Total @InvoiceNumber	

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
