
CREATE   VIEW App.vwPeriodEndListing
AS
SELECT        TOP (100) PERCENT App.tbYear.YearNumber, App.tbYear.Description, App.tbYear.InsertedBy AS YearInsertedBy, App.tbYear.InsertedOn AS YearInsertedOn, App.tbYearPeriod.StartOn, App.tbMonth.MonthName, 
                         App.tbYearPeriod.InsertedBy AS PeriodInsertedBy, App.tbYearPeriod.InsertedOn AS PeriodInsertedOn, Cash.tbStatus.CashStatus
FROM            Cash.tbStatus INNER JOIN
                         App.tbYear INNER JOIN
                         App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON Cash.tbStatus.CashStatusCode = App.tbYearPeriod.CashStatusCode
ORDER BY App.tbYearPeriod.StartOn;
