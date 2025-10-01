CREATE   VIEW Cash.vwFlowCategories
AS
	WITH trade_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 0
	), trade_cat AS
	(
		SELECT trade_type.CashTypeCode, trade_type.CashType, cats.CategoryCode, cats.Category, cats.CashModeCode, cats.DisplayOrder 
		FROM trade_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(trade_type.CashTypeCode) cat
			) cats
	), cash_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 2
	), cash_cat AS
	(
		SELECT cash_type.CashTypeCode, 
		cash_type.CashType, cats.CategoryCode, cats.Category, cats.CashModeCode, cats.DisplayOrder
		FROM cash_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(cash_type.CashTypeCode) cat
			) cats
	),  tax_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 1
	), tax_cat AS
	(
		SELECT tax_type.CashTypeCode, 
		tax_type.CashType, cats.CategoryCode, cats.Category, cats.CashModeCode, cats.DisplayOrder
		FROM tax_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(tax_type.CashTypeCode) cat
			) cats
	), catagories_unsorted AS
	(
		SELECT CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashModeCode 
		FROM trade_cat
		UNION
		SELECT 1 CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashModeCode 
		FROM cash_cat
		UNION
		SELECT 2 CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashModeCode 
		FROM tax_cat
	)
	SELECT CashTypeCode, ROW_NUMBER() OVER (ORDER BY CashTypeCode, DisplayOrder) EntryId,
		CashType, CategoryCode, Category, CashModeCode
	FROM catagories_unsorted;
