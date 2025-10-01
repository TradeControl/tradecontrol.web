
CREATE   VIEW App.vwActivePeriod
AS
SELECT App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn, App.tbYear.Description, App.tbMonth.MonthNumber, App.tbMonth.MonthName, fnActivePeriod.EndOn
FROM            App.tbYear INNER JOIN
                         App.fnActivePeriod() AS fnActivePeriod INNER JOIN
                         App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON fnActivePeriod.StartOn = App.tbYearPeriod.StartOn AND fnActivePeriod.YearNumber = App.tbYearPeriod.YearNumber ON 
                         App.tbYear.YearNumber = App.tbYearPeriod.YearNumber
