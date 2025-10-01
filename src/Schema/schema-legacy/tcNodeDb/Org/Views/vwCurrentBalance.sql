CREATE   VIEW Org.vwCurrentBalance
AS
	WITH current_balance AS
	(
		SELECT AccountCode, MAX(RowNumber) CurrentBalanceRow
		FROM Org.vwStatement
		GROUP BY AccountCode
	)
	SELECT org_statement.AccountCode, org_statement.Balance
	FROM Org.vwStatement org_statement
		JOIN current_balance ON org_statement.AccountCode = current_balance.AccountCode 
			AND org_statement.RowNumber = current_balance.CurrentBalanceRow
