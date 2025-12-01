CREATE   VIEW App.vwYears
AS
	SELECT App.tbYear.YearNumber, CONCAT(App.tbMonth.MonthName, ' ', App.tbYear.YearNumber) StartMonth, App.tbYear.CashStatusCode, Cash.tbStatus.CashStatus, App.tbYear.Description, App.tbYear.InsertedBy, App.tbYear.InsertedOn
	FROM App.tbYear 
		JOIN Cash.tbStatus ON App.tbYear.CashStatusCode = Cash.tbStatus.CashStatusCode 
		JOIN App.tbMonth ON App.tbYear.StartMonth = App.tbMonth.MonthNumber AND App.tbYear.StartMonth = App.tbMonth.MonthNumber;
