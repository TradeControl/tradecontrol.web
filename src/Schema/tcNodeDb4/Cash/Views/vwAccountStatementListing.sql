CREATE VIEW Cash.vwAccountStatementListing
AS
	SELECT        App.tbYear.YearNumber, Subject.tbSubject.SubjectName AS Bank, Subject.tbAccount.AccountCode, Subject.tbAccount.AccountName, Subject.tbAccount.SortCode, Subject.tbAccount.AccountNumber, CONCAT(App.tbYear.Description, SPACE(1), 
							 App.tbMonth.MonthName) AS PeriodName, Cash.vwAccountStatement.StartOn, CAST(Cash.vwAccountStatement.EntryNumber AS INT) EntryNumber, Cash.vwAccountStatement.PaymentCode, Cash.vwAccountStatement.PaidOn, 
							 Cash.vwAccountStatement.SubjectName, Cash.vwAccountStatement.PaymentReference, Cash.vwAccountStatement.PaidInValue, Cash.vwAccountStatement.PaidOutValue, 
							 Cash.vwAccountStatement.PaidBalance, Cash.vwAccountStatement.CashCode, 
							 Cash.vwAccountStatement.CashDescription, Cash.vwAccountStatement.TaxDescription, Cash.vwAccountStatement.UserName, Cash.vwAccountStatement.SubjectCode, 
							 Cash.vwAccountStatement.TaxCode
	FROM            App.tbYearPeriod INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 Cash.vwAccountStatement INNER JOIN
							 Subject.tbAccount ON Cash.vwAccountStatement.AccountCode = Subject.tbAccount.AccountCode INNER JOIN
							 Subject.tbSubject ON Subject.tbAccount.SubjectCode = Subject.tbSubject.SubjectCode ON App.tbYearPeriod.StartOn = Cash.vwAccountStatement.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber;
