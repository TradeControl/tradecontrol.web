
CREATE   PROCEDURE Invoice.proc_DefaultDocType
	(
		@InvoiceNumber nvarchar(20),
		@DocTypeCode smallint OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @InvoiceTypeCode smallint

			SELECT  @InvoiceTypeCode = InvoiceTypeCode
			FROM         Invoice.tbInvoice
			WHERE     (InvoiceNumber = @InvoiceNumber)
	
			SET @DocTypeCode = CASE @InvoiceTypeCode
									WHEN 0 THEN 4
									WHEN 1 THEN 5							
									WHEN 3 THEN 6
									ELSE 4
									END
							
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
