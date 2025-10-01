CREATE VIEW Cash.vwCategoryTotalCandidates
AS
	SELECT Cash.tbCategory.CategoryCode, Cash.tbCategory.Category, Cash.tbCategoryType.CategoryType, Cash.tbType.CashType, Cash.tbMode.CashMode
	FROM   Cash.tbCategory INNER JOIN
				Cash.tbCategoryType ON Cash.tbCategory.CategoryTypeCode = Cash.tbCategoryType.CategoryTypeCode INNER JOIN
				Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode INNER JOIN
				Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode
	WHERE        (Cash.tbCategory.CashTypeCode < 2) AND (Cash.tbCategory.IsEnabled <> 0)
	UNION
	SELECT CategoryCode, Category, CategoryType, CashType, CashMode
	FROM Cash.vwCategoryCapital
