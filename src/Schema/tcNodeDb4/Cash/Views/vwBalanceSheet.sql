CREATE VIEW Cash.vwBalanceSheet
AS
	WITH balance_sheets AS
	(

		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetSubjects
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAccounts
		UNION 
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAssets
		UNION 
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetTax
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetVat

	), balance_sheet_unordered AS
	(
		SELECT 
			balance_sheet_periods.AssetCode, balance_sheet_periods.AssetName,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.CashPolarityCode 
				ELSE balance_sheets.CashPolarityCode 
			END CashPolarityCode, LiquidityLevel,
			balance_sheet_periods.StartOn,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN 0 
				ELSE balance_sheets.Balance 
			END Balance,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.IsEntry 
				ELSE CAST(1 as bit) 
			END IsEntry
		FROM Cash.vwBalanceSheetPeriods balance_sheet_periods
			LEFT OUTER JOIN balance_sheets
				ON balance_sheet_periods.AssetCode = balance_sheets.AssetCode
					AND balance_sheet_periods.AssetName = balance_sheets.AssetName
					AND balance_sheet_periods.CashPolarityCode = balance_sheets.CashPolarityCode
					AND balance_sheet_periods.StartOn = balance_sheets.StartOn
	), balance_sheet_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY CashPolarityCode desc, LiquidityLevel desc, AssetName, StartOn) EntryNumber,
			AssetCode, AssetName, CashPolarityCode, LiquidityLevel, StartOn, Balance, IsEntry
		FROM balance_sheet_unordered
	), balance_sheet_ranked AS
	(
		SELECT *, 
		RANK() OVER (PARTITION BY AssetName, CashPolarityCode, IsEntry ORDER BY EntryNumber) RNK
		FROM balance_sheet_ordered
	), balance_sheet_grouped AS
	(
		SELECT EntryNumber, AssetCode, AssetName, CashPolarityCode, LiquidityLevel, StartOn, Balance, IsEntry,
		MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AssetName, CashPolarityCode ORDER BY EntryNumber) RNK
		FROM balance_sheet_ranked
	)
	SELECT EntryNumber, AssetCode, AssetName, CashPolarityCode, LiquidityLevel, balance_sheet_grouped.StartOn, 
		year_period.YearNumber, year_period.MonthNumber, IsEntry,
		CASE IsEntry WHEN 0 THEN
			MAX(Balance) OVER (PARTITION BY AssetName, CashPolarityCode, RNK ORDER BY EntryNumber) +
			MIN(Balance) OVER (PARTITION BY AssetName, CashPolarityCode, RNK ORDER BY EntryNumber) 
		ELSE
			Balance
		END AS Balance
	FROM balance_sheet_grouped
		JOIN App.tbYearPeriod year_period ON balance_sheet_grouped.StartOn = year_period.StartOn;

