CREATE   VIEW Cash.vwCategoryCapital
AS
	SELECT DISTINCT category.CategoryCode, category.Category, category.DisplayOrder, cat_type.CategoryType, cash_type.CashType, cash_mode.CashPolarity,
		cat_type.CategoryTypeCode, cash_type.CashTypeCode, cash_mode.CashPolarityCode
	FROM Subject.tbAccount account
		JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
		JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		JOIN Cash.tbType cash_type ON category.CashTypeCode = cash_type.CashTypeCode
		JOIN Cash.tbCategoryType cat_type ON category.CategoryTypeCode = cat_type.CategoryTypeCode
		JOIN Cash.tbPolarity cash_mode ON category.CashPolarityCode = cash_mode.CashPolarityCode
	WHERE (AccountTypeCode = 2);
