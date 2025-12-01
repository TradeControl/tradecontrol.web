
CREATE   VIEW App.vwVatTaxCashCodes
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
		WHERE Cash.tbCategory.CashTypeCode = 0
	), cashcode_candidates AS
	(
		SELECT     ChildCode, CashCode
		FROM category_relations
		WHERE     ( CategoryTypeCode = 1) AND ( ParentCode = (SELECT VatCategoryCode FROM App.tbOptions))

		UNION ALL

		SELECT     category_relations.ChildCode, category_relations.CashCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CashCode FROM cashcode_candidates
		UNION
		SELECT CashCode FROM category_relations WHERE ParentCode = (SELECT VatCategoryCode FROM App.tbOptions)
	)
	SELECT CashCode FROM cashcode_selected WHERE NOT CashCode IS NULL;

