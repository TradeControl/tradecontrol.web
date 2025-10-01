CREATE VIEW Invoice.vwRegisterCashCodes
AS
	WITH cash_codes AS
	(
		SELECT StartOn, CashCode, CashDescription, CashPolarityCode, CAST(SUM(InvoiceValue) as float) AS TotalInvoiceValue, CAST(SUM(TaxValue) as float) AS TotalTaxValue
		FROM            Invoice.vwRegisterDetail
		GROUP BY StartOn, CashCode, CashDescription, CashPolarityCode	
	)
	SELECT cash_codes.StartOn, CONCAT(financial_year.[Description], ' ', app_month.MonthName) PeriodName, CashPolarity,
		CashCode, CashDescription, TotalInvoiceValue, TotalTaxValue, TotalInvoiceValue + TotalTaxValue as TotalValue		
	FROM cash_codes
		JOIN Cash.tbPolarity cash_mode ON cash_codes.CashPolarityCode = cash_mode.CashPolarityCode
		JOIN App.tbYearPeriod year_period ON cash_codes.StartOn = year_period.StartOn
		JOIN App.tbMonth app_month ON year_period.MonthNumber = app_month.MonthNumber
		JOIN App.tbYear financial_year ON year_period.YearNumber = financial_year.YearNumber;
