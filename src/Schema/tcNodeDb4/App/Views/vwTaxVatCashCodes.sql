CREATE VIEW App.vwTaxVatCashCodes
AS
	WITH vat_enabled AS
	(
		SELECT IsEnabled
		FROM Cash.tbTaxType
		WHERE TaxTypeCode = 1
	),
	category_relations AS
	(
		SELECT
			ct.ParentCode,
			ct.ChildCode,
			cat.CategoryTypeCode,
			code.CashCode
		FROM Cash.tbCategoryTotal ct
			INNER JOIN Cash.tbCategory cat
				ON ct.ChildCode = cat.CategoryCode
			LEFT OUTER JOIN Cash.tbCode code
				ON cat.CategoryCode = code.CategoryCode
			CROSS JOIN vat_enabled ve
		WHERE ve.IsEnabled = 1
		  AND cat.CashTypeCode = 0
		  AND cat.IsEnabled = 1
		  AND (code.CashCode IS NULL OR code.IsEnabled = 1)
	),
	cashcode_candidates AS
	(
		SELECT ChildCode, CashCode
		FROM category_relations
		WHERE (CategoryTypeCode = 1)
		  AND (ParentCode = (SELECT VatCategoryCode FROM App.tbOptions))

		UNION ALL

		SELECT cr.ChildCode, cr.CashCode
		FROM category_relations cr
			JOIN cashcode_candidates cc
				ON cr.ParentCode = cc.ChildCode
	),
	cashcode_selected AS
	(
		SELECT CashCode FROM cashcode_candidates
		UNION
		SELECT CashCode FROM category_relations WHERE ParentCode = (SELECT VatCategoryCode FROM App.tbOptions)
	)
	SELECT CashCode
	FROM cashcode_selected
	WHERE CashCode IS NOT NULL;
