CREATE PROCEDURE Cash.proc_TaxAdjustmentGet
(
	@StartOn datetime,
	@TaxTypeCode smallint,
	@TaxAdjustment decimal(18, 5) OUTPUT
)
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
		WHERE @StartOn >= due_dates.PayFrom AND @StartOn < due_dates.PayTo;

		SELECT @StartOn = MAX(StartOn)
		FROM App.tbYearPeriod
		WHERE StartOn < @PayTo;

		SELECT @TaxAdjustment =
			CASE @TaxTypeCode
				WHEN 0 THEN TaxAdjustment
				WHEN 1 THEN VatAdjustment
				ELSE 0
			END
		FROM App.tbYearPeriod
		WHERE StartOn = @StartOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
