CREATE   PROCEDURE Cash.proc_TaxAdjustment (@StartOn datetime, @TaxTypeCode smallint, @TaxAdjustment decimal(18, 5))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE 		
			@PayTo datetime,
			@PayFrom datetime;

		SELECT 
			@PayFrom = PayFrom,
			@PayTo = PayTo 
		FROM Cash.fnTaxTypeDueDates(@TaxTypeCode) due_dates 
		WHERE @StartOn >= due_dates.PayFrom AND @StartOn < due_dates.PayTo

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN 0 ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN 0 ELSE VatAdjustment END
		WHERE StartOn >= @PayFrom AND StartOn < @PayTo;

		SELECT @StartOn = MAX(StartOn)
		FROM App.tbYearPeriod
		WHERE StartOn < @PayTo;

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN @TaxAdjustment ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN @TaxAdjustment ELSE VatAdjustment END
		WHERE StartOn = @StartOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
