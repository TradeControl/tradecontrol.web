
CREATE   VIEW Cash.vwCategoryCodesTrade
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashModeCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 0);
