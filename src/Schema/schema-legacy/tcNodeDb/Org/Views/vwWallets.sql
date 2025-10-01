CREATE VIEW Org.vwWallets
AS
	SELECT        Org.tbAccount.CashAccountCode, Org.tbAccount.CashAccountName, Org.tbAccount.CashCode, Org.tbAccount.CoinTypeCode
	FROM            Org.tbAccount INNER JOIN
							 App.tbOptions ON Org.tbAccount.AccountCode = App.tbOptions.AccountCode LEFT OUTER JOIN
							 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Org.tbAccount.AccountTypeCode = 0) AND Org.tbAccount.CoinTypeCode < 2;
