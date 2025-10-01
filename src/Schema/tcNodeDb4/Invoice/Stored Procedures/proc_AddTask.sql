
CREATE   PROCEDURE Invoice.proc_AddProject 
	(
	@InvoiceNumber nvarchar(20),
	@ProjectCode nvarchar(20)	
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@InvoiceTypeCode smallint
		, @InvoiceQuantity float
		, @QuantityInvoiced float

		IF EXISTS(SELECT     InvoiceNumber, ProjectCode
				  FROM         Invoice.tbProject
				  WHERE     (InvoiceNumber = @InvoiceNumber) AND (ProjectCode = @ProjectCode))
			RETURN
		
		SELECT   @InvoiceTypeCode = InvoiceTypeCode
		FROM         Invoice.tbInvoice
		WHERE     (InvoiceNumber = @InvoiceNumber) 

		IF EXISTS(SELECT     SUM( Invoice.tbProject.Quantity) AS QuantityInvoiced
				  FROM         Invoice.tbProject INNER JOIN
										Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
										Invoice.tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
			BEGIN
			SELECT TOP 1 @QuantityInvoiced = isnull(SUM( Invoice.tbProject.Quantity), 0)
			FROM         Invoice.tbProject INNER JOIN
					tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
					tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)				
			END
		ELSE
			SET @QuantityInvoiced = 0
		
		IF @InvoiceTypeCode = 1 or @InvoiceTypeCode = 3
			BEGIN
			IF EXISTS(SELECT     SUM( Invoice.tbProject.Quantity) AS QuantityInvoiced
					  FROM         Invoice.tbProject INNER JOIN
											tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
					  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
											tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
				BEGIN
				SELECT TOP 1 @InvoiceQuantity = isnull(@QuantityInvoiced, 0) - isnull(SUM( Invoice.tbProject.Quantity), 0)
				FROM         Invoice.tbProject INNER JOIN
						tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
						tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)										
				END
			ELSE
				SET @InvoiceQuantity = isnull(@QuantityInvoiced, 0)
			END
		ELSE
			BEGIN
			SELECT  @InvoiceQuantity = Quantity - isnull(@QuantityInvoiced, 0)
			FROM         Project.tbProject
			WHERE     (ProjectCode = @ProjectCode)
			END
			
		IF isnull(@InvoiceQuantity, 0) <= 0
			SET @InvoiceQuantity = 1
		
		INSERT INTO Invoice.tbProject
							  (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT     @InvoiceNumber AS InvoiceNumber, ProjectCode, @InvoiceQuantity AS Quantity, UnitCharge * @InvoiceQuantity AS InvoiceValue, CashCode, 
							  TaxCode
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)

		UPDATE Project.tbProject
		SET ActionedOn = CURRENT_TIMESTAMP
		WHERE ProjectCode = @ProjectCode;
	
		EXEC Invoice.proc_Total @InvoiceNumber	

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
