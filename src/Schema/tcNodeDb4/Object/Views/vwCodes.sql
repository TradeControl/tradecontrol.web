
CREATE   VIEW Object.vwCodes
AS
SELECT        Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, Object.tbObject.CashCode
FROM            Object.tbObject LEFT OUTER JOIN
                         Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode;
