
CREATE   PROCEDURE Task.proc_DefaultInvoiceType
	(
		@TaskCode nvarchar(20),
		@InvoiceTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE @CashModeCode smallint

		IF EXISTS(SELECT     CashModeCode
				  FROM         Task.vwCashMode
				  WHERE     (TaskCode = @TaskCode))
			SELECT   @CashModeCode = CashModeCode
			FROM         Task.vwCashMode
			WHERE     (TaskCode = @TaskCode)			          
		ELSE
			SET @CashModeCode = 1
		
		IF @CashModeCode = 0
			SET @InvoiceTypeCode = 2
		ELSE
			SET @InvoiceTypeCode = 0
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
