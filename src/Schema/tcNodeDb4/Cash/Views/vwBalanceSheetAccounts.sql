CREATE VIEW Cash.vwBalanceSheetAccounts
AS
	WITH cash_accounts AS
	(
		SELECT AccountCode, CashCode 
		FROM Subject.tbAccount
		WHERE AccountTypeCode = 0
	)
	, account_periods AS
	(
		SELECT AccountCode AS AccountCode, CashCode, App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM App.tbYearPeriod 
			JOIN App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
			CROSS JOIN  cash_accounts
		WHERE (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
	), last_entries AS
	(
		SELECT account_statement.AccountCode, account_statement.StartOn, MAX(account_statement.EntryNumber) As EntryNumber
		FROM Cash.vwAccountStatement account_statement 
			JOIN cash_accounts ON account_statement.AccountCode = cash_accounts.AccountCode
		GROUP BY account_statement.AccountCode, account_statement.StartOn
	)
	, closing_balance AS
	(
		SELECT account_statement.AccountCode,  account_statement.StartOn, account_statement.PaidBalance 
		FROM last_entries 
			JOIN Cash.vwAccountStatement account_statement ON last_entries.AccountCode = account_statement.AccountCode
				AND last_entries.EntryNumber = account_statement.EntryNumber
	)
	, statement_ordered AS
	(
		SELECT 
			account_periods.AccountCode, account_periods.CashCode,
			ROW_NUMBER() OVER (PARTITION BY account_periods.AccountCode ORDER BY account_periods.StartOn) EntryNumber,
			account_periods.YearNumber, account_periods.StartOn, CAST(COALESCE(closing_balance.PaidBalance, 0) as float) Balance,
			CASE WHEN closing_balance.AccountCode IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END IsEntry
		FROM account_periods
			LEFT OUTER JOIN closing_balance 
				ON account_periods.AccountCode = closing_balance.AccountCode AND account_periods.StartOn = closing_balance.StartOn
	)
	, statement_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM statement_ordered
	)
	, statement_grouped AS
	(
		SELECT EntryNumber, AccountCode, CashCode, YearNumber, StartOn, Balance, IsEntry,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM statement_ranked
	), account_balances AS
	(
		SELECT AccountCode, CashCode, StartOn, 
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) 
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
				THEN (SELECT AccountCode FROM Cash.vwCurrentAccount) 
				ELSE (SELECT AccountCode FROM Cash.vwReserveAccount) 
			END AS AssetCode,
			1 CashPolarityCode,
			CASE WHEN (CashCode IS NULL) THEN 2 ELSE 3 END AssetTypeCode, StartOn, Balance
		FROM account_polarity
	)
	SELECT AssetCode, asset_type.AssetType AssetName, CashPolarityCode, asset_type.AssetTypeCode, StartOn, Balance
	FROM account_base
		JOIN Cash.tbAssetType asset_type ON account_base.AssetTypeCode = asset_type.AssetTypeCode;

