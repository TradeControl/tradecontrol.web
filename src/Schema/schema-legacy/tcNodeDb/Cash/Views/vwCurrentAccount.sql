CREATE VIEW Cash.vwCurrentAccount
AS
	SELECT TOP (1) Org.tbAccount.CashAccountCode, Org.tbAccount.LiquidityLevel, Org.tbAccount.CashAccountName, Org.tbAccount.AccountNumber, Org.tbAccount.SortCode, Org.tbAccount.AccountCode, Org.tbOrg.AccountName
	FROM            Org.tbAccount INNER JOIN
							 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Org.tbOrg ON Org.tbAccount.AccountCode = Org.tbOrg.AccountCode
	WHERE        (Cash.tbCategory.CashTypeCode = 2) AND (Org.tbAccount.AccountTypeCode = 0) AND (Org.tbAccount.AccountClosed = 0)
	ORDER BY Org.tbAccount.CashAccountCode;
