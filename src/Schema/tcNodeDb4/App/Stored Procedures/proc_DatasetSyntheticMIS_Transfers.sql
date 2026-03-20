CREATE PROCEDURE App.proc_DatasetSyntheticMIS_Transfers
(
	@FloatRatio decimal(18,7) = 0.25
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @FloatRatio IS NULL OR @FloatRatio <= 0 OR @FloatRatio >= 1
		THROW 51340, 'DatasetSyntheticMIS_Transfers: @FloatRatio must be between 0 and 1.', 1;

	DECLARE
		@CurrentAccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.vwCurrentAccount),
		@StartOn date = (SELECT MIN(CAST(StartOn AS date)) FROM App.tbYearPeriod),
		@CurrentPeriodStartOn date =
		(
			SELECT MIN(CAST(StartOn AS date))
			FROM App.tbYearPeriod
			WHERE CashStatusCode = 1
		);

	IF @CurrentAccountCode IS NULL OR @StartOn IS NULL OR @CurrentPeriodStartOn IS NULL
		THROW 51341, 'DatasetSyntheticMIS_Transfers: unable to resolve period/account anchors.', 1;

	-- Sweep at month end for each month that has fully completed (end < current period start)
	IF OBJECT_ID('tempdb..#SweepMonths') IS NOT NULL
		DROP TABLE #SweepMonths;

	;WITH m AS
	(
		SELECT CAST(@StartOn AS date) AS MonthStartOn
		UNION ALL
		SELECT DATEADD(month, 1, MonthStartOn)
		FROM m
		WHERE DATEADD(month, 1, MonthStartOn) < @CurrentPeriodStartOn
	)
	SELECT
		MonthStartOn,
		CAST(EOMONTH(MonthStartOn) AS date) AS MonthEndOn,
		DATEADD(month, 1, CAST(MonthStartOn AS date)) AS NextMonthStartOn,
		CAST(EOMONTH(DATEADD(month, 1, MonthStartOn)) AS date) AS NextMonthEndOn
	INTO #SweepMonths
	FROM m
	OPTION (MAXRECURSION 1000);

	DECLARE
		@MonthEndOn date,
		@NextMonthStartOn date,
		@NextMonthEndOn date,
		@CurrentBalance decimal(18,5),
		@FloatByRatio decimal(18,5),
		@NextMonthPayOut decimal(18,5),
		@FloatToKeep decimal(18,5),
		@TransferAmount decimal(18,5);

	DECLARE curSweep CURSOR LOCAL FAST_FORWARD FOR
		SELECT MonthEndOn, NextMonthStartOn, NextMonthEndOn
		FROM #SweepMonths
		ORDER BY MonthEndOn;

	OPEN curSweep;
	FETCH NEXT FROM curSweep INTO @MonthEndOn, @NextMonthStartOn, @NextMonthEndOn;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- Current account balance as-of month end (posted only)
		SELECT @CurrentBalance = ISNULL(SUM(Paid), 0)
		FROM
		(
			SELECT
				CASE
					WHEN PaidInValue > 0 THEN CAST(PaidInValue AS decimal(18,5))
					ELSE CAST(PaidOutValue * -1 AS decimal(18,5))
				END AS Paid
			FROM Cash.tbPayment p
			WHERE p.AccountCode = @CurrentAccountCode
			  AND p.PaymentStatusCode = 1
			  AND CAST(p.PaidOn AS date) <= @MonthEndOn
		) b;

		-- Next-month expected expenditure (posted PayOut only)
		SELECT @NextMonthPayOut = ISNULL(SUM(CAST(PaidOutValue AS decimal(18,5))), 0)
		FROM Cash.tbPayment p
		WHERE p.AccountCode = @CurrentAccountCode
		  AND p.PaymentStatusCode = 1
		  AND CAST(p.PaidOn AS date) >= @NextMonthStartOn
		  AND CAST(p.PaidOn AS date) <= @NextMonthEndOn;

		SET @FloatByRatio = CAST(ROUND(@CurrentBalance * @FloatRatio, 2) AS decimal(18,5));
		SET @FloatToKeep = CASE WHEN @FloatByRatio < @NextMonthPayOut THEN @FloatByRatio ELSE @NextMonthPayOut END;

		SET @TransferAmount = @CurrentBalance - @FloatToKeep;
		IF @TransferAmount < 0 SET @TransferAmount = 0;

		-- Transfer if meaningful (avoid penny noise)
		IF @TransferAmount >= 1.00
		BEGIN
			EXEC App.proc_DatasetSyntheticMIS_Transfer
				@PaidOn = @MonthEndOn,
				@Amount = @TransferAmount;
		END

		FETCH NEXT FROM curSweep INTO @MonthEndOn, @NextMonthStartOn, @NextMonthEndOn;
	END

	CLOSE curSweep;
	DEALLOCATE curSweep;
GO
