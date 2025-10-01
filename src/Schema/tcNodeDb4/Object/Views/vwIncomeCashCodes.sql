CREATE   VIEW Object.vwIncomeCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Object.vwCandidateCashCodes
	WHERE CashPolarityCode = 1 AND CashTypeCode = 0
