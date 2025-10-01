
CREATE   VIEW Cash.vwCategoryCodesExpressions
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 2);
