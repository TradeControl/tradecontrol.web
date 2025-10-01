CREATE   PROCEDURE Project.proc_FullyInvoiced
	(
	@ProjectCode nvarchar(20),
	@IsFullyInvoiced bit = 0 output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceValue decimal(18, 5)
			, @TotalCharge decimal(18, 5)

		SELECT @InvoiceValue = SUM(InvoiceValue)
		FROM         Invoice.tbProject
		WHERE     (ProjectCode = @ProjectCode)
	
	
		SELECT @TotalCharge = SUM(TotalCharge)
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)
	
		IF (@TotalCharge = @InvoiceValue)
			SET @IsFullyInvoiced = 1
		ELSE
			SET @IsFullyInvoiced = 0	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
