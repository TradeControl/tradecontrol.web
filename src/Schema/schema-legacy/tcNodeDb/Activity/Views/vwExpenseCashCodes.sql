CREATE   VIEW Activity.vwExpenseCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Activity.vwCandidateCashCodes
	WHERE CashModeCode = 0 AND CashTypeCode = 0
