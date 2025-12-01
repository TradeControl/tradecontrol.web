CREATE   VIEW Subject.vwCurrentBalance
AS
	WITH current_balance AS
	(
		SELECT SubjectCode, MAX(RowNumber) CurrentBalanceRow
		FROM Subject.vwStatement
		GROUP BY SubjectCode
	)
	SELECT Subject_statement.SubjectCode, Subject_statement.Balance
	FROM Subject.vwStatement Subject_statement
		JOIN current_balance ON Subject_statement.SubjectCode = current_balance.SubjectCode 
			AND Subject_statement.RowNumber = current_balance.CurrentBalanceRow
