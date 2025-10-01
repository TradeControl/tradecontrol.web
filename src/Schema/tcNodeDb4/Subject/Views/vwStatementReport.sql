CREATE   VIEW Subject.vwStatementReport
AS
	SELECT  asset.SubjectCode, o.SubjectName, asset.RowNumber, App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, asset.StartOn, asset.TransactedOn, asset.Reference, asset.StatementType, asset.Charge, asset.Balance
	FROM            Subject.vwAssetStatement AS asset INNER JOIN
							 Subject.tbSubject AS o ON o.SubjectCode = asset.SubjectCode INNER JOIN
							 App.tbYearPeriod ON asset.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
