CREATE   VIEW Cash.vwCategoryCapital
AS
	SELECT DISTINCT category.CategoryCode, category.Category, category.DisplayOrder, cat_type.CategoryType, cash_type.CashType, cash_mode.CashMode,
		cat_type.CategoryTypeCode, cash_type.CashTypeCode, cash_mode.CashModeCode
	FROM Org.tbAccount account
		JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
		JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		JOIN Cash.tbType cash_type ON category.CashTypeCode = cash_type.CashTypeCode
		JOIN Cash.tbCategoryType cat_type ON category.CategoryTypeCode = cat_type.CategoryTypeCode
		JOIN Cash.tbMode cash_mode ON category.CashModeCode = cash_mode.CashModeCode
	WHERE (AccountTypeCode = 2);
