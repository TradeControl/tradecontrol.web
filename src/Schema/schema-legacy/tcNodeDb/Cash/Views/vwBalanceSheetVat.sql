CREATE VIEW Cash.vwBalanceSheetVat
AS
	WITH vat_due AS 
	(	
		SELECT StartOn, SUM(VatDue) AS VatDue
		FROM Cash.vwTaxVatSummary 
		GROUP BY StartOn
	)
	, vat_paid AS
	(
		SELECT vat_due.StartOn, VatDue - VatAdjustment VatDue, 0 VatPaid
		FROM vat_due
			JOIN App.tbYearPeriod year_period ON vat_due.StartOn = year_period.StartOn

		UNION

		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Cash.tbPayment.PaidOn) ORDER BY StartOn DESC) AS StartOn, 
			0 As VatDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS VatPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType vat_codes ON Cash.tbPayment.CashCode = vat_codes.CashCode	
		WHERE (vat_codes.TaxTypeCode = 1)
	), vat_unordered AS
	(
		SELECT StartOn, SUM(VatDue) VatDue, SUM(VatPaid) VatPaid
		FROM vat_paid
		GROUP BY StartOn
	), vat_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY StartOn, VatDue) AS RowNumber,
			StartOn, VatDue, VatPaid
		FROM vat_unordered
	), vat_balance AS
	(
		SELECT RowNumber, StartOn, VatDue, VatPaid,
			SUM(VatDue+VatPaid) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM vat_ordered
	)
	, vat_statement AS
	(
		SELECT RowNumber, StartOn, CAST(VatDue as float) VatDue, CAST(VatPaid as float) VatPaid, CAST(Balance as decimal(18,5)) Balance
		FROM vat_balance
		WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT tax_type.AssetCode, tax_type.AssetName, 
		CAST(0 as smallint) CashModeCode,  
		CAST(1 as smallint) AssetTypeCode,  
		StartOn, 
		Balance * -1 Balance 
	FROM vat_statement
		CROSS JOIN
		(
			SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName
			FROM Cash.tbTaxType
			WHERE TaxTypeCode = 1
		) tax_type;

