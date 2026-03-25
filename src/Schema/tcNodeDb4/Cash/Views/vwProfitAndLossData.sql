CREATE VIEW Cash.vwProfitAndLossData
AS
	WITH active_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), category_data AS
	(
		SELECT
			cc.CategoryCode,
			cc.CashTypeCode,
			periods.CashCode,
			periods.StartOn,
			CASE cc.CashPolarityCode WHEN 0 THEN periods.InvoiceValue * -1 ELSE periods.InvoiceValue END AS InvoiceValue
		FROM App.vwCorpTaxCashCodes cc
			JOIN Cash.tbCategory category ON cc.CategoryCode = category.CategoryCode
			JOIN Cash.tbPeriod periods ON cc.CashCode = periods.CashCode
			JOIN active_periods ON active_periods.StartOn = periods.StartOn
	)
	SELECT CategoryCode, CashTypeCode, StartOn, SUM(InvoiceValue) InvoiceValue
	FROM category_data
	GROUP BY CategoryCode, CashTypeCode, StartOn;
