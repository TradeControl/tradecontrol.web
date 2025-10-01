CREATE VIEW App.vwYearPeriod
AS
	SELECT App.tbYear.Description, App.tbMonth.MonthName, App.tbYearPeriod.CashStatusCode, Cash.tbStatus.CashStatus, App.tbYearPeriod.YearNumber, App.tbYearPeriod.MonthNumber, App.tbYearPeriod.StartOn, App.tbYearPeriod.RowVer
	FROM App.tbYearPeriod INNER JOIN
		App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
		App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
		Cash.tbStatus ON App.tbYearPeriod.CashStatusCode = Cash.tbStatus.CashStatusCode;
