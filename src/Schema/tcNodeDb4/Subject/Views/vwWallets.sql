CREATE VIEW Subject.vwWallets
AS
	SELECT        Subject.tbAccount.AccountCode, Subject.tbAccount.AccountName, Subject.tbAccount.CashCode, Subject.tbAccount.CoinTypeCode
	FROM            Subject.tbAccount INNER JOIN
							 App.tbOptions ON Subject.tbAccount.SubjectCode = App.tbOptions.SubjectCode LEFT OUTER JOIN
							 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Subject.tbAccount.AccountTypeCode = 0) AND Subject.tbAccount.CoinTypeCode < 2;
