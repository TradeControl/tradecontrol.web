CREATE   VIEW Subject.vwAssetStatementAudit
AS
	SELECT App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, App.tbYearPeriod.StartOn, asset_statement.SubjectCode, account.SubjectName, asset_statement.RowNumber, asset_statement.TransactedOn, asset_statement.Charge, asset_statement.Balance
	FROM  Subject.vwAssetStatement AS asset_statement INNER JOIN
			Subject.tbSubject AS account ON asset_statement.SubjectCode = account.SubjectCode INNER JOIN
			App.tbYearPeriod ON asset_statement.StartOn = App.tbYearPeriod.StartOn INNER JOIN
			App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
			App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
