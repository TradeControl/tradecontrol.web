CREATE VIEW Cash.vwCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbPolarity.CashPolarityCode, Cash.tbPolarity.CashPolarity, Cash.tbCode.TaxCode, Cash.tbCategory.CashTypeCode, Cash.tbType.CashType
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0)
