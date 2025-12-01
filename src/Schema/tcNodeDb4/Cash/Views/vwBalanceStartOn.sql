CREATE   VIEW Cash.vwBalanceStartOn
AS
	SELECT MIN(App.tbYearPeriod.StartOn) StartOn
	FROM  App.tbYearPeriod 
		JOIN App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
	WHERE (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.CashStatusCode < 3)
