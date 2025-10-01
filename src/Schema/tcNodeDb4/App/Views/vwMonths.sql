
CREATE     VIEW [App].[vwMonths]
AS
	SELECT DISTINCT CAST(App.tbYearPeriod.StartOn AS decimal) AS StartOn, App.tbMonth.MonthName, App.tbYearPeriod.MonthNumber
	FROM         App.tbYearPeriod INNER JOIN
						  App.fnActivePeriod() AS fnSystemActivePeriod ON App.tbYearPeriod.YearNumber = fnSystemActivePeriod.YearNumber INNER JOIN
						  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
