
CREATE   VIEW Cash.vwCategoryTrade
AS
SELECT        CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, RowVer
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 0) AND (CashTypeCode = 0)
