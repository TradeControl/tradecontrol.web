CREATE   VIEW Cash.vwProfitAndLossData
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, 
			Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode, Cash.tbCategory.CashTypeCode, Cash.tbCategory.CashModeCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	), categories AS
	(
		SELECT CategoryCode, CashTypeCode
		FROM  Cash.tbCategory category 
		WHERE (CategoryTypeCode = 1)
			AND NOT EXISTS (SELECT * FROM App.tbOptions o WHERE o.VatCategoryCode = category.CategoryCode) 
			
	), cashcode_candidates AS
	(
		SELECT categories.CategoryCode, ChildCode, CashCode, CashModeCode
		FROM category_relations
			JOIN categories ON category_relations.ParentCode = categories.CategoryCode		

		UNION ALL

		SELECT  cashcode_candidates.CategoryCode, category_relations.ChildCode, category_relations.CashCode, category_relations.CashModeCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CategoryCode, CashCode, CashModeCode FROM cashcode_candidates
		UNION
		SELECT ParentCode CategoryCode, CashCode, CashModeCode FROM category_relations WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	), category_cash_codes AS
	(
		SELECT DISTINCT CategoryCode, CashCode, CashModeCode
		FROM cashcode_selected WHERE NOT CashCode IS NULL
	), active_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), category_data AS
	(
		SELECT category_cash_codes.CategoryCode, CashTypeCode, periods.CashCode, periods.StartOn, 
			CASE category_cash_codes.CashModeCode WHEN 0 THEN periods.InvoiceValue * -1 ELSE InvoiceValue END InvoiceValue
		FROM category_cash_codes 
			JOIN Cash.tbCategory category ON category_cash_codes.CategoryCode = category.CategoryCode
			JOIN Cash.tbPeriod periods ON category_cash_codes.CashCode = periods.CashCode
			JOIN active_periods ON active_periods.StartOn = periods.StartOn
	)
	SELECT CategoryCode, CashTypeCode, StartOn, SUM(InvoiceValue) InvoiceValue
	FROM category_data
	GROUP BY CategoryCode, CashTypeCode, StartOn;
