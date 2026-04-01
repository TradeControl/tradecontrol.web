CREATE PROCEDURE App.proc_DatasetSyntheticMIS_SoleTraderPersonalTaxRate
(
	@IsCompany bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany <> 0
		RETURN;

	-- Annual profit is derived from the same net profit basis as corp-tax totals
	;WITH yearly_profit AS
	(
		SELECT
			yp.YearNumber,
			AnnualProfit = CAST(SUM(COALESCE(ct.NetProfit, 0)) AS decimal(18, 2))
		FROM App.tbYearPeriod yp
			LEFT JOIN Cash.vwTaxBizTotalsByPeriod ct
				ON ct.StartOn = yp.StartOn
		GROUP BY yp.YearNumber
	),
	yearly_rate AS
	(
		SELECT
			YearNumber,
			[BusinessTaxRate] = CAST(Cash.fnPersonalEffectiveRateCalculator(AnnualProfit) AS decimal(9, 6))
		FROM yearly_profit
	)
	UPDATE yp
	SET yp.[BusinessTaxRate] = yr.[BusinessTaxRate]
	FROM App.tbYearPeriod yp
		JOIN yearly_rate yr
			ON yr.YearNumber = yp.YearNumber;
GO
