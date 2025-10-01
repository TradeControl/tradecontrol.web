CREATE VIEW Cash.vwBankCashCodes
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.TaxCode, Cash.tbCategory.CashModeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode LEFT OUTER JOIN
							 Cash.vwTransferCodeLookup ON Cash.tbCode.CashCode = Cash.vwTransferCodeLookup.CashCode
	WHERE        (Cash.tbCategory.CashTypeCode = 2) AND (Cash.vwTransferCodeLookup.CashCode IS NULL)
