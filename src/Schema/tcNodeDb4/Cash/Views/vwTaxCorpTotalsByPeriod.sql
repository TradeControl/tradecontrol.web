CREATE VIEW Cash.vwTaxCorpTotalsByPeriod
AS
	WITH invoiced_Projects AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.InvoiceValue * - 1 ELSE Invoice.tbProject.InvoiceValue END AS InvoiceValue
		FROM            Invoice.tbProject INNER JOIN
								 App.vwCorpTaxCashCodes CashCodes  ON Invoice.tbProject.CashCode = CashCodes.CashCode INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE CashTypeCode < 3
	), invoiced_items AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,  
							  CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue
		FROM         Invoice.tbItem INNER JOIN
							  App.vwCorpTaxCashCodes CashCodes ON Invoice.tbItem.CashCode = CashCodes.CashCode INNER JOIN
							  Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE CashTypeCode < 3
	), assets AS
	(
		SELECT cash_codes.CashCode, financial_periods.StartOn, 
			CASE cash_codes.CashPolarityCode WHEN 0 THEN financial_periods.InvoiceValue * -1 ELSE financial_periods.InvoiceValue END AssetValue
		FROM App.vwCorpTaxCashCodes cash_codes
			JOIN Cash.tbPeriod financial_periods
				ON cash_codes.CashCode = financial_periods.CashCode
		WHERE cash_codes.CashTypeCode = 2
	), netprofits AS	
	(
		SELECT StartOn, SUM(InvoiceValue) NetProfit 
		FROM invoiced_Projects 
		GROUP BY StartOn
		
		UNION
		
		SELECT StartOn, SUM(InvoiceValue) NetProfit 
		FROM invoiced_items 
		GROUP BY StartOn

		UNION

		SELECT StartOn, SUM(AssetValue) NetProfit
		FROM assets
		GROUP BY StartOn
	)
	, netprofit_consolidated AS
	(
		SELECT StartOn, SUM(NetProfit) AS NetProfit FROM netprofits GROUP BY StartOn
	)
	SELECT App.tbYearPeriod.StartOn, netprofit_consolidated.NetProfit, 
							netprofit_consolidated.NetProfit * App.tbYearPeriod.CorporationTaxRate + App.tbYearPeriod.TaxAdjustment AS CorporationTax, 
							App.tbYearPeriod.TaxAdjustment
	FROM         netprofit_consolidated INNER JOIN
							App.tbYearPeriod ON netprofit_consolidated.StartOn = App.tbYearPeriod.StartOn;

