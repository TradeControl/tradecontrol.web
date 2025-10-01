CREATE VIEW Cash.vwBalanceSheetAccounts
AS
	WITH cash_accounts AS
	(
		SELECT CashAccountCode, CashCode 
		FROM Org.tbAccount
		WHERE AccountTypeCode = 0
	)
	, account_periods AS
	(
		SELECT CashAccountCode AS CashAccountCode, CashCode, App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM App.tbYearPeriod 
			JOIN App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
			CROSS JOIN  cash_accounts
		WHERE (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
	), last_entries AS
	(
		SELECT account_statement.CashAccountCode, account_statement.StartOn, MAX(account_statement.EntryNumber) As EntryNumber
		FROM Cash.vwAccountStatement account_statement 
			JOIN cash_accounts ON account_statement.CashAccountCode = cash_accounts.CashAccountCode
		GROUP BY account_statement.CashAccountCode, account_statement.StartOn
	)
	, closing_balance AS
	(
		SELECT account_statement.CashAccountCode,  account_statement.StartOn, account_statement.PaidBalance 
		FROM last_entries 
			JOIN Cash.vwAccountStatement account_statement ON last_entries.CashAccountCode = account_statement.CashAccountCode
				AND last_entries.EntryNumber = account_statement.EntryNumber
	)
	, statement_ordered AS
	(
		SELECT 
			account_periods.CashAccountCode, account_periods.CashCode,
			ROW_NUMBER() OVER (PARTITION BY account_periods.CashAccountCode ORDER BY account_periods.StartOn) EntryNumber,
			account_periods.YearNumber, account_periods.StartOn, CAST(COALESCE(closing_balance.PaidBalance, 0) as float) Balance,
			CASE WHEN closing_balance.CashAccountCode IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END IsEntry
		FROM account_periods
			LEFT OUTER JOIN closing_balance 
				ON account_periods.CashAccountCode = closing_balance.CashAccountCode AND account_periods.StartOn = closing_balance.StartOn
	)
	, statement_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY CashAccountCode ORDER BY EntryNumber) RNK
		FROM statement_ordered
	)
	, statement_grouped AS
	(
		SELECT EntryNumber, CashAccountCode, CashCode, YearNumber, StartOn, Balance, IsEntry,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY CashAccountCode ORDER BY EntryNumber) RNK
		FROM statement_ranked
	), account_balances AS
	(
		SELECT CashAccountCode, CashCode, StartOn, 
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY CashAccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY CashAccountCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END
			AS Balance		
		FROM statement_grouped
	), account_polarity AS
	(
		SELECT CashCode, StartOn, SUM(Balance) Balance
		FROM account_balances
		GROUP BY CashCode, StartOn
	), account_base AS
	(
		SELECT 
			CASE WHEN NOT (CashCode IS NULL) 
				THEN (SELECT CashAccountCode FROM Cash.vwCurrentAccount) 
				ELSE (SELECT CashAccountCode FROM Cash.vwReserveAccount) 
			END AS AssetCode,
			1 CashModeCode,
			CASE WHEN (CashCode IS NULL) THEN 2 ELSE 3 END AssetTypeCode, StartOn, Balance
		FROM account_polarity
	)
	SELECT AssetCode, asset_type.AssetType AssetName, CashModeCode, asset_type.AssetTypeCode, StartOn, Balance
	FROM account_base
		JOIN Cash.tbAssetType asset_type ON account_base.AssetTypeCode = asset_type.AssetTypeCode;

