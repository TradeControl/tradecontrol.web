CREATE   VIEW Object.vwExpenseCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Object.vwCandidateCashCodes
	WHERE CashPolarityCode = 0 AND CashTypeCode = 0
