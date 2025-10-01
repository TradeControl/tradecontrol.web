CREATE VIEW App.vwPeriods
AS
	SELECT TOP (100) PERCENT App.tbYear.YearNumber, App.tbYearPeriod.MonthNumber, App.tbYearPeriod.StartOn, App.tbYear.Description + SPACE(1) + App.tbMonth.MonthName AS Description, App.tbYearPeriod.CashStatusCode, App.tbYearPeriod.RowVer
	FROM            App.tbYearPeriod INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
	WHERE        (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.CashStatusCode < 3)

