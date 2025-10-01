
CREATE   VIEW Cash.vwPeriods
   AS
SELECT     Cash.tbCode.CashCode, App.tbYearPeriod.StartOn
FROM         App.tbYearPeriod CROSS JOIN
                      Cash.tbCode
