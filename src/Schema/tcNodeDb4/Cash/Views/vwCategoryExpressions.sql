

CREATE VIEW [Cash].[vwCategoryExpressions]
AS
	SELECT cat.DisplayOrder
		, cat.CategoryCode
		, cat.Category
		, expr.Expression
		, expr.Format
		, expr.SyntaxTypeCode
		, temp.Template
	FROM Cash.tbCategory cat
		JOIN Cash.tbCategoryExp expr
			ON cat.CategoryCode = expr.CategoryCode
		LEFT JOIN Cash.tbCategoryExprFormat temp
			ON expr.Format = temp.TemplateCode
	WHERE (cat.CategoryTypeCode = 2) and (cat.IsEnabled != 0)
