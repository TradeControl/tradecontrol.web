CREATE VIEW Cash.vwBudgetDataEntry
AS
SELECT        TOP (100) PERCENT App.tbYearPeriod.YearNumber, Cash.tbPeriod.CashCode, Cash.tbPeriod.StartOn, App.tbMonth.MonthName, Cash.tbPeriod.ForecastValue, Cash.tbPeriod.ForecastTax, Cash.tbPeriod.Note, 
                         Cash.tbPeriod.RowVer
FROM            App.tbYearPeriod INNER JOIN
                         Cash.tbPeriod ON App.tbYearPeriod.StartOn = Cash.tbPeriod.StartOn INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
