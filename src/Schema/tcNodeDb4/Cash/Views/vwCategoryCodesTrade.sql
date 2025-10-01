
CREATE   VIEW Cash.vwCategoryCodesTrade
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 0);
