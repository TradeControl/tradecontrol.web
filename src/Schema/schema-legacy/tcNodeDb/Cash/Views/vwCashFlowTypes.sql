
CREATE   VIEW Cash.vwCashFlowTypes
AS
SELECT        CashTypeCode, CashType
FROM            Cash.tbType
WHERE        (CashTypeCode < 2)
