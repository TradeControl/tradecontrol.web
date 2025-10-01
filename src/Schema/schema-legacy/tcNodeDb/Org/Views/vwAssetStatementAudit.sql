CREATE   VIEW Org.vwAssetStatementAudit
AS
	SELECT App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, App.tbYearPeriod.StartOn, asset_statement.AccountCode, account.AccountName, asset_statement.RowNumber, asset_statement.TransactedOn, asset_statement.Charge, asset_statement.Balance
	FROM  Org.vwAssetStatement AS asset_statement INNER JOIN
			Org.tbOrg AS account ON asset_statement.AccountCode = account.AccountCode INNER JOIN
			App.tbYearPeriod ON asset_statement.StartOn = App.tbYearPeriod.StartOn INNER JOIN
			App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
			App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
