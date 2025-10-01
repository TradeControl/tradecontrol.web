CREATE   VIEW Org.vwStatementReport
AS
	SELECT  asset.AccountCode, o.AccountName, asset.RowNumber, App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, asset.StartOn, asset.TransactedOn, asset.Reference, asset.StatementType, asset.Charge, asset.Balance
	FROM            Org.vwAssetStatement AS asset INNER JOIN
							 Org.tbOrg AS o ON o.AccountCode = asset.AccountCode INNER JOIN
							 App.tbYearPeriod ON asset.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
