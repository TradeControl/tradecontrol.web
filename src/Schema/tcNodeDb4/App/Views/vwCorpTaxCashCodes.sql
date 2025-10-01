CREATE VIEW App.vwCorpTaxCashCodes
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, 
			Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode, Cash.tbCategory.CashTypeCode, Cash.tbCategory.CashPolarityCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	), cashcode_candidates AS
	(
		SELECT     ChildCode, CashCode, CashTypeCode, CashPolarityCode
		FROM category_relations
		WHERE     ( CategoryTypeCode = 1) AND ( ParentCode = (SELECT NetProfitCode FROM App.tbOptions))

		UNION ALL

		SELECT     category_relations.ChildCode, category_relations.CashCode, category_relations.CashTypeCode, category_relations.CashPolarityCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CashCode, CashTypeCode, CashPolarityCode FROM cashcode_candidates
		UNION
		SELECT CashCode, CashTypeCode, CashPolarityCode FROM category_relations WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	)
	SELECT CashCode, CashTypeCode, CashPolarityCode
	FROM cashcode_selected WHERE NOT CashCode IS NULL;
