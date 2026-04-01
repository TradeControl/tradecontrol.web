CREATE   PROCEDURE App.proc_TaxRates(@StartOn datetime, @EndOn datetime, @BusinessTaxRate real)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY	
		UPDATE App.tbYearPeriod
		SET BusinessTaxRate = @BusinessTaxRate
		WHERE StartOn >= @StartOn AND StartOn <= @EndOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
