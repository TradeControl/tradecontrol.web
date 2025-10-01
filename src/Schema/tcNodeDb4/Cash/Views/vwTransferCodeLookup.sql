CREATE VIEW Cash.vwTransferCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbPolarity.CashPolarity, Cash.tbPolarity.CashPolarityCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode LEFT OUTER JOIN
							 Subject.tbAccount ON Cash.tbCode.CashCode = Subject.tbAccount.CashCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCategory.CashTypeCode = 2) AND (Cash.tbPolarity.CashPolarityCode < 2) AND (Subject.tbAccount.AccountCode IS NULL)
