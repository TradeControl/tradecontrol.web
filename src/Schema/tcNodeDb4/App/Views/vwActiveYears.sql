
CREATE   VIEW App.vwActiveYears
   AS
SELECT     TOP 100 PERCENT App.tbYear.YearNumber, App.tbYear.Description, Cash.tbStatus.CashStatus
FROM         App.tbYear INNER JOIN
                      Cash.tbStatus ON App.tbYear.CashStatusCode = Cash.tbStatus.CashStatusCode
WHERE     (App.tbYear.CashStatusCode < 3)
ORDER BY App.tbYear.YearNumber
