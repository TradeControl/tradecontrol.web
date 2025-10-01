
CREATE   VIEW Cash.vwCategoryExpressions
AS
	SELECT     TOP 100 PERCENT Cash.tbCategory.DisplayOrder, Cash.tbCategory.CategoryCode, Cash.tbCategory.Category, Cash.tbCategoryExp.Expression, 
						  Cash.tbCategoryExp.Format
	FROM         Cash.tbCategory INNER JOIN
						  Cash.tbCategoryExp ON Cash.tbCategory.CategoryCode = Cash.tbCategoryExp.CategoryCode
	WHERE     (Cash.tbCategory.CategoryTypeCode = 2)
