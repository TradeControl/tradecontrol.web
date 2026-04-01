CREATE VIEW Cash.vwTaxBizTotals
AS
	WITH totals AS
	(
		SELECT App.tbYearPeriod.YearNumber, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn) AS PeriodYear, App.tbYear.Description, 
						  App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR(App.tbYearPeriod.StartOn))) AS Period, App.tbYearPeriod.BusinessTaxRate, 
						  App.tbYearPeriod.BusinessTaxAdjustment, SUM(netprofit_totals.NetProfit) AS NetProfit, SUM(netprofit_totals.BusinessTax) AS BusinessTax
		FROM       Cash.vwTaxBizTotalsByPeriod  netprofit_totals INNER JOIN
							  App.tbYearPeriod ON netprofit_totals.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
		WHERE     (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
		GROUP BY App.tbYearPeriod.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn), 
							  App.tbYearPeriod.BusinessTaxRate, App.tbYearPeriod.BusinessTaxAdjustment
	)
	SELECT YearNumber, StartOn, PeriodYear, Description, Period, BusinessTaxRate, BusinessTaxAdjustment, CAST(NetProfit AS decimal(18, 5)) NetProfit, CAST(BusinessTax AS decimal(18, 5)) BusinessTax
	FROM totals;

