

CREATE   VIEW Cash.vwCategoryCodesTotals
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 1);
