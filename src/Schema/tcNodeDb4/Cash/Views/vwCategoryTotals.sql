
CREATE   VIEW Cash.vwCategoryTotals
AS
	SELECT CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbCategory
	WHERE       (CategoryTypeCode = 1)
