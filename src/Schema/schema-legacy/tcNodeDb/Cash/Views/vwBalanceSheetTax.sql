CREATE VIEW Cash.vwBalanceSheetTax
AS
	WITH tax_dates AS
	(
		SELECT (SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE StartOn < PayTo ORDER BY StartOn DESC) PayOn, 
			PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), period_totals AS
	(
		SELECT (SELECT PayOn FROM tax_dates WHERE totals.StartOn >= PayFrom AND totals.StartOn < PayTo) AS StartOn, CorporationTax
		FROM Cash.vwTaxCorpTotalsByPeriod totals
	), tax_entries AS
	(
		SELECT StartOn, SUM(CorporationTax) AS TaxDue, 0 AS TaxPaid
		FROM period_totals
		WHERE NOT StartOn IS NULL
		GROUP BY StartOn
		
		UNION

		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Cash.tbPayment.PaidOn) ORDER BY StartOn DESC) AS StartOn, 
			0 As TaxDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS TaxPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType tt ON Cash.tbPayment.CashCode = tt.CashCode
		WHERE (tt.TaxTypeCode = 0)

	)
	, tax_balances AS
	(
		SELECT StartOn, TaxDue, TaxPaid,
			SUM(TaxDue + TaxPaid) OVER (ORDER BY StartOn, TaxDue ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM tax_entries
	), tax_statement AS
	(
		SELECT StartOn, CAST(TaxDue AS decimal(18, 5)) TaxDue, CAST(TaxPaid AS decimal(18, 5)) TaxPaid, CAST(Balance AS decimal(18, 5)) Balance FROM tax_balances 
		WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT tax_type.AssetCode, tax_type.AssetName, 
		CAST(0 as smallint) CashModeCode,  
		CAST(1 as smallint) AssetTypeCode,  
		StartOn, 		
		CASE WHEN Balance < 0 THEN 0 ELSE Balance * -1 END Balance 
	FROM tax_statement
		CROSS JOIN
		(
			SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName
			FROM Cash.tbTaxType
			WHERE TaxTypeCode = 0
		) tax_type;
