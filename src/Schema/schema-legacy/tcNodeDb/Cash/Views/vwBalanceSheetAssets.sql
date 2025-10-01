CREATE VIEW Cash.vwBalanceSheetAssets
AS
	WITH asset_statements AS
	(
		SELECT account_statement.CashAccountCode, COALESCE(StartOn, (SELECT MIN(StartOn) FROM App.tbYearPeriod)) StartOn, EntryNumber, PaidBalance
		FROM Cash.vwAccountStatement account_statement
			JOIN Org.tbAccount account ON account_statement.CashAccountCode = account.CashAccountCode
		WHERE account.AccountTypeCode = 2 AND account.AccountClosed = 0 
	), asset_last_tx AS
	(
		SELECT CashAccountCode, MAX(EntryNumber) EntryNumber
		FROM asset_statements
		GROUP BY CashAccountCode, StartOn
	)
	, asset_polarity AS
	(
		SELECT asset_statements.CashAccountCode, asset_statements.StartOn, SUM(asset_statements.PaidBalance) Balance, CAST(1 as bit) IsEntry
		FROM asset_statements
			JOIN asset_last_tx ON asset_statements.CashAccountCode = asset_last_tx.CashAccountCode AND asset_statements.EntryNumber = asset_last_tx.EntryNumber
		GROUP BY asset_statements.CashAccountCode, asset_statements.StartOn
	), asset_periods AS
	(
		SELECT CashAccountCode, StartOn,  0 Balance, CAST(0 as bit) IsEntry
		FROM App.tbYearPeriod year_periods
			CROSS JOIN Org.tbAccount account
		WHERE account.AccountTypeCode = 2 AND account.AccountClosed = 0
	), asset_unordered AS
	(
		SELECT asset_periods.CashAccountCode, asset_periods.StartOn,
			CASE WHEN asset_polarity.CashAccountCode IS NULL THEN asset_periods.Balance ELSE asset_polarity.Balance END Balance,
			CASE WHEN asset_polarity.CashAccountCode IS NULL THEN asset_periods.IsEntry ELSE asset_polarity.IsEntry END IsEntry
		FROM asset_periods
			LEFT OUTER JOIN asset_polarity
				ON asset_periods.CashAccountCode = asset_polarity.CashAccountCode
					AND asset_periods.StartOn = asset_polarity.StartOn
	), asset_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY CashAccountCode, StartOn) EntryNumber,
			CashAccountCode, StartOn, Balance, IsEntry
		FROM asset_unordered
	)
	, asset_ranked AS
	(
		SELECT *, 
		RANK() OVER (PARTITION BY CashAccountCode, IsEntry ORDER BY EntryNumber) RNK
		FROM asset_ordered
	)
	, asset_grouped AS
	(
		SELECT EntryNumber, CashAccountCode, StartOn, Balance, IsEntry,
		MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY CashAccountCode ORDER BY EntryNumber) RNK
		FROM asset_ranked
	), asset_base AS
	(
		SELECT EntryNumber, CashAccountCode, StartOn, IsEntry,
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY CashAccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY CashAccountCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END AS Balance
		FROM asset_grouped
	), asset_accounts AS
	(
		SELECT CashAccountCode, CashAccountName, CashModeCode
		FROM Org.tbAccount accounts
			JOIN Cash.tbCode cash_code ON accounts.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
		WHERE AccountTypeCode = 2 AND AccountClosed = 0
	)
	SELECT asset_accounts.CashAccountCode AssetCode, CashAccountName AssetName, CashModeCode, 4 AssetTypeCode, StartOn, Balance
	FROM asset_base
		JOIN asset_accounts ON asset_base.CashAccountCode = asset_accounts.CashAccountCode;
