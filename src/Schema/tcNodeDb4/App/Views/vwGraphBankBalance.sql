CREATE VIEW App.vwGraphBankBalance
AS
	WITH last_entries AS
	(
		SELECT     AccountCode, StartOn, MAX(EntryNumber) AS LastEntry
		FROM         Cash.vwAccountStatement
		GROUP BY AccountCode, StartOn
		HAVING      (NOT (StartOn IS NULL))
	), closing_balance AS
	(
		SELECT        Subject.tbAccount.AccountCode, Subject.tbAccount.CashCode, last_entries.StartOn, SUM(Cash.vwAccountStatement.PaidBalance) AS ClosingBalance
		FROM            last_entries INNER JOIN
								 Cash.vwAccountStatement ON last_entries.AccountCode = Cash.vwAccountStatement.AccountCode AND 
								 last_entries.StartOn = Cash.vwAccountStatement.StartOn AND 
								 last_entries.LastEntry = Cash.vwAccountStatement.EntryNumber INNER JOIN
								 Subject.tbAccount ON last_entries.AccountCode = Subject.tbAccount.AccountCode
		WHERE Subject.tbAccount.AccountTypeCode = 0
		GROUP BY Subject.tbAccount.AccountCode, Subject.tbAccount.CashCode, last_entries.StartOn
	)
	SELECT        Format(closing_balance.StartOn, 'yyyy-MM') AS PeriodOn, SUM(closing_balance.ClosingBalance) AS SumOfClosingBalance
	FROM            closing_balance INNER JOIN
							 Cash.tbCode ON closing_balance.CashCode = Cash.tbCode.CashCode
	WHERE        (closing_balance.StartOn > DATEADD(m, - 6, CURRENT_TIMESTAMP))
	GROUP BY Format(closing_balance.StartOn, 'yyyy-MM');
