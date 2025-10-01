CREATE FUNCTION Cash.fnFlowBankBalances (@CashAccountCode NVARCHAR(10))
RETURNS TABLE
AS
	RETURN
	WITH account_periods AS
	(
		SELECT    @CashAccountCode AS CashAccountCode, App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
	), last_entries AS
	(
		SELECT account_statement.CashAccountCode, account_statement.StartOn, MAX(account_statement.EntryNumber) As EntryNumber
		FROM Cash.vwAccountStatement account_statement 
		WHERE account_statement.CashAccountCode = @CashAccountCode
		GROUP BY account_statement.CashAccountCode, account_statement.StartOn
	), closing_balance AS
	(
		SELECT account_statement.CashAccountCode,  account_statement.StartOn, account_statement.PaidBalance 
		FROM last_entries 
			JOIN Cash.vwAccountStatement account_statement ON last_entries.CashAccountCode = account_statement.CashAccountCode
				AND last_entries.EntryNumber = account_statement.EntryNumber
	), statement_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY account_periods.StartOn) EntryNumber,
			account_periods.CashAccountCode, account_periods.YearNumber, account_periods.StartOn, CAST(COALESCE(closing_balance.PaidBalance, 0) as float) Balance,
			CASE WHEN closing_balance.CashAccountCode IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END IsEntry
		FROM account_periods
			LEFT OUTER JOIN closing_balance 
				ON account_periods.CashAccountCode = closing_balance.CashAccountCode AND account_periods.StartOn = closing_balance.StartOn
	), statement_ranked AS
	(
		SELECT *,
			RANK() OVER (ORDER BY EntryNumber) RNK
		FROM statement_ordered
	), statement_grouped AS
	(
		SELECT EntryNumber, CashAccountCode, YearNumber, StartOn, Balance, IsEntry,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (ORDER BY EntryNumber) RNK
		FROM statement_ranked
	)
	SELECT CashAccountCode, YearNumber, StartOn, 
		CASE IsEntry WHEN 0 THEN
			MAX(Balance) OVER (PARTITION BY RNK ORDER BY EntryNumber) +
			MIN(Balance) OVER (PARTITION BY RNK ORDER BY EntryNumber) 
		ELSE
			Balance
		END
		AS Balance		
	FROM statement_grouped;
