CREATE VIEW App.vwCorpTaxCashCodes
AS
	WITH category_relations AS
	(
		SELECT
			Cash.tbCategoryTotal.ParentCode,
			Cash.tbCategoryTotal.ChildCode,
			Cash.tbCategory.CategoryTypeCode,
			Cash.tbCategory.CategoryCode,
			Cash.tbCode.CashCode,
			Cash.tbCategory.CashTypeCode,
			Cash.tbCategory.CashPolarityCode
		FROM Cash.tbCategoryTotal
			INNER JOIN Cash.tbCategory
				ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode
			LEFT OUTER JOIN Cash.tbCode
				ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	),
	cashcode_candidates AS
	(
		SELECT
			ChildCode,
			CategoryCode,
			CashCode,
			CashTypeCode,
			CashPolarityCode
		FROM category_relations
		WHERE (CategoryTypeCode = 1)
		  AND (ParentCode = (SELECT NetProfitCode FROM App.tbOptions))

		UNION ALL

		SELECT
			cr.ChildCode,
			cr.CategoryCode,
			cr.CashCode,
			cr.CashTypeCode,
			cr.CashPolarityCode
		FROM category_relations cr
			JOIN cashcode_candidates cc
				ON cr.ParentCode = cc.ChildCode
	),
	cashcode_selected AS
	(
		SELECT CategoryCode, CashCode, CashTypeCode, CashPolarityCode
		FROM cashcode_candidates

		UNION

		SELECT CategoryCode, CashCode, CashTypeCode, CashPolarityCode
		FROM category_relations
		WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	)
	SELECT CategoryCode, CashCode, CashTypeCode, CashPolarityCode
	FROM cashcode_selected
	WHERE NOT CashCode IS NULL;
