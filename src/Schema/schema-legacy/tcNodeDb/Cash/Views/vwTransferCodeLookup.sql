CREATE VIEW Cash.vwTransferCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbMode.CashMode, Cash.tbMode.CashModeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode LEFT OUTER JOIN
							 Org.tbAccount ON Cash.tbCode.CashCode = Org.tbAccount.CashCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCategory.CashTypeCode = 2) AND (Cash.tbMode.CashModeCode < 2) AND (Org.tbAccount.CashAccountCode IS NULL)
