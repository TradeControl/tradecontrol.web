
CREATE   VIEW Cash.vwCategoryCodesExpressions
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashModeCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 2);
