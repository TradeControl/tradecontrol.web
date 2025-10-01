
CREATE   PROCEDURE Project.proc_DefaultInvoiceType
	(
		@ProjectCode nvarchar(20),
		@InvoiceTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE @CashPolarityCode smallint

		IF EXISTS(SELECT     CashPolarityCode
				  FROM         Project.vwCashPolarity
				  WHERE     (ProjectCode = @ProjectCode))
			SELECT   @CashPolarityCode = CashPolarityCode
			FROM         Project.vwCashPolarity
			WHERE     (ProjectCode = @ProjectCode)			          
		ELSE
			SET @CashPolarityCode = 1
		
		IF @CashPolarityCode = 0
			SET @InvoiceTypeCode = 2
		ELSE
			SET @InvoiceTypeCode = 0
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
