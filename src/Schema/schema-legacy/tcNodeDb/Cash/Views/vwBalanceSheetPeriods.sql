CREATE VIEW Cash.vwBalanceSheetPeriods
AS
	WITH financial_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), assets AS
	(
		SELECT CashAccountCode AssetCode, CashAccountName AssetName, LiquidityLevel, CAST(4 as smallint) AssetTypeCode, 
			category.CashModeCode,
			YearNumber, StartOn
		FROM Org.tbAccount account
			JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
			CROSS JOIN financial_periods
		WHERE (AccountTypeCode= 2) AND (AccountClosed = 0)
	), cash AS
	(
		SELECT CashAccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode, CAST(1 as smallint) CashModeCode, YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwCurrentAccount 
			CROSS JOIN financial_periods
		WHERE AssetTypeCode = 3
	), bank AS
	(
		SELECT CashAccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode, CAST(1 as smallint) CashModeCode, YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwReserveAccount 
			CROSS JOIN financial_periods
		WHERE AssetTypeCode = 2
	), orgs AS
	(
		SELECT CashAccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode,
			CAST(CASE AssetTypeCode WHEN 0 THEN 1 ELSE 0 END as smallint) CashModeCode,
			YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwCurrentAccount
			CROSS JOIN financial_periods
		WHERE AssetTypeCode BETWEEN 0 AND 1
	), tax AS
	(
		SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName, CAST(1 as smallint) LiquidityLevel, CAST(1 as smallint) AssetTypeCode, CAST(0 as smallint) CashModeCode,
			YearNumber, StartOn
		FROM Cash.tbTaxType
			CROSS JOIN financial_periods
		WHERE TaxTypeCode BETWEEN 0 AND 1

	), asset_code_periods AS
	(
		SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM assets
		UNION 
		SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM cash
		UNION
		SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM bank
		UNION
		SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM orgs
		UNION
		SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM tax
	)
	SELECT AssetCode, AssetName, CashModeCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn, CAST(0 as bit) IsEntry
	FROM asset_code_periods;
