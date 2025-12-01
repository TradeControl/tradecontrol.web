CREATE   VIEW Cash.vwProfitAndLossByYear
AS
	SELECT financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category, category.CashTypeCode, SUM(profit_data.InvoiceValue) InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
	GROUP BY financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category, category.CashTypeCode;
