CREATE   VIEW Activity.vwIncomeCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Activity.vwCandidateCashCodes
	WHERE CashModeCode = 1 AND CashTypeCode = 0
