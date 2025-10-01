

CREATE   VIEW Cash.vwCategoryCodesTotals
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashModeCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 1);
