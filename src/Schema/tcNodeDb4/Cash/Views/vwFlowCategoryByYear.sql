CREATE   VIEW Cash.vwFlowCategoryByYear
AS
	SELECT CategoryCode, CashCode, YearNumber, SUM(InvoiceValue) InvoiceValue
	FROM Cash.vwFlowCategoryByPeriod
	GROUP BY CategoryCode, CashCode, YearNumber
