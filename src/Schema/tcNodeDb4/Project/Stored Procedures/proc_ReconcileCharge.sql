CREATE   PROCEDURE Project.proc_ReconcileCharge
	(
	@ProjectCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @InvoiceValue decimal(18, 5)

		SELECT @InvoiceValue = SUM(InvoiceValue)
		FROM         Invoice.tbProject
		WHERE     (ProjectCode = @ProjectCode)

		UPDATE    Project.tbProject
		SET              TotalCharge = @InvoiceValue, UnitCharge = @InvoiceValue / Quantity
		WHERE     (ProjectCode = @ProjectCode)	
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
