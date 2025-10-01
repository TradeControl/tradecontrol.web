CREATE VIEW Cash.vwTaxLossesCarriedForward
AS
	WITH tax_dates AS
	(
		SELECT PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), period_totals AS
	(
		SELECT (SELECT PayTo FROM tax_dates WHERE totals.StartOn >= PayFrom AND totals.StartOn < PayTo) AS StartOn, CorporationTax
		FROM Cash.vwTaxCorpTotalsByPeriod totals
	), tax_entries AS
	(
		SELECT StartOn, SUM(CorporationTax) AS TaxDue, 0 AS TaxPaid
		FROM period_totals
		WHERE NOT StartOn IS NULL
		GROUP BY StartOn
		
		UNION

		SELECT Cash.tbPayment.PaidOn AS StartOn, 0 As TaxDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS TaxPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType tt ON Cash.tbPayment.CashCode = tt.CashCode
		WHERE (tt.TaxTypeCode = 0)

	), tax_statement AS
	(
		SELECT StartOn, TaxDue, TaxPaid,
			SUM(TaxDue + TaxPaid) OVER (ORDER BY StartOn, TaxDue ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM tax_entries
	), profit_statement AS
	(
		SELECT tax_statement.StartOn, CAST(TaxDue AS decimal(18, 5)) TaxDue, CAST(Balance AS decimal(18, 5)) TaxBalance,  
			CAST(Balance / CorporationTaxRate AS decimal(18, 5)) LossesCarriedForward
		FROM tax_statement 
			JOIN App.tbYearPeriod yp ON tax_statement.StartOn = yp.StartOn
		WHERE tax_statement.StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT CONCAT(y.[Description], ' ', mn.MonthName) YearEndDescription,
		profit_statement.StartOn, TaxDue, TaxBalance, 
		CASE WHEN LossesCarriedForward < 0 THEN ABS(LossesCarriedForward) ELSE 0 END LossesCarriedForward		
	FROM profit_statement
		JOIN App.tbYearPeriod yp ON profit_statement.StartOn = yp.StartOn
		JOIN App.tbYear y ON yp.YearNumber - 1 = y.YearNumber
		JOIN App.tbMonth mn ON yp.MonthNumber = mn.MonthNumber;
