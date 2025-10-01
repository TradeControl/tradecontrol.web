CREATE   VIEW Cash.vwProfitAndLossByPeriod
AS
	SELECT category.CategoryCode, category.Category, category.CashTypeCode, periods.YearNumber, periods.MonthNumber, category.DisplayOrder, financial_year.Description,
		year_month.MonthName, profit_data.StartOn, profit_data.InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
		JOIN App.tbMonth year_month ON periods.MonthNumber = year_month.MonthNumber;
