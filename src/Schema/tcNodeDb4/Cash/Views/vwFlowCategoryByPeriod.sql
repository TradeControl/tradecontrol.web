CREATE   VIEW Cash.vwFlowCategoryByPeriod
AS
	SELECT cats.CategoryCode, cash_codes.CashCode, cash_codes.CashDescription,	
		YearNumber, year_period.StartOn, year_period.MonthNumber, CASE cats.CashPolarityCode WHEN 0 THEN InvoiceValue * -1 ELSE InvoiceValue END InvoiceValue
	FROM Cash.tbCategory cats
		JOIN Cash.tbCode cash_codes ON cats.CategoryCode = cash_codes.CategoryCode
		JOIN Cash.tbPeriod cash_periods ON cash_codes.CashCode = cash_periods.CashCode
		JOIN App.tbYearPeriod year_period ON cash_periods.StartOn = year_period.StartOn
	WHERE cash_codes.IsEnabled <> 0
