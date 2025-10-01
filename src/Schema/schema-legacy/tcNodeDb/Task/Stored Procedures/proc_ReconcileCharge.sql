CREATE   PROCEDURE Task.proc_ReconcileCharge
	(
	@TaskCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @InvoiceValue decimal(18, 5)

		SELECT @InvoiceValue = SUM(InvoiceValue)
		FROM         Invoice.tbTask
		WHERE     (TaskCode = @TaskCode)

		UPDATE    Task.tbTask
		SET              TotalCharge = @InvoiceValue, UnitCharge = @InvoiceValue / Quantity
		WHERE     (TaskCode = @TaskCode)	
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
