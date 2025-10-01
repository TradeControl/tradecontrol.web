
CREATE   VIEW Activity.vwCodes
AS
SELECT        Activity.tbActivity.ActivityCode, Activity.tbActivity.UnitOfMeasure, Activity.tbActivity.CashCode
FROM            Activity.tbActivity LEFT OUTER JOIN
                         Cash.tbCode ON Activity.tbActivity.CashCode = Cash.tbCode.CashCode;
