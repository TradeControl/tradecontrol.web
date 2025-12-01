CREATE VIEW Cash.vwCategoryTotalCandidates
AS
	SELECT Cash.tbCategory.CategoryCode, Cash.tbCategory.Category, Cash.tbCategoryType.CategoryType, Cash.tbType.CashType, Cash.tbPolarity.CashPolarity
	FROM   Cash.tbCategory INNER JOIN
				Cash.tbCategoryType ON Cash.tbCategory.CategoryTypeCode = Cash.tbCategoryType.CategoryTypeCode INNER JOIN
				Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode INNER JOIN
				Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Cash.tbCategory.CashTypeCode < 2) AND (Cash.tbCategory.IsEnabled <> 0)
	UNION
	SELECT CategoryCode, Category, CategoryType, CashType, CashPolarity
	FROM Cash.vwCategoryCapital
