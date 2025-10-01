CREATE VIEW Org.vwCashAccounts
AS
SELECT        Org.tbAccount.CashAccountCode, Org.tbOrg.AccountCode, Org.tbAccount.CashAccountName, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbAccount.OpeningBalance, Org.tbAccount.CurrentBalance, 
                         Org.tbAccount.SortCode, Org.tbAccount.AccountNumber, Org.tbAccount.AccountClosed, Org.tbAccount.AccountTypeCode, Org.tbAccountType.AccountType, Org.tbAccount.CashCode, Cash.tbCode.CashDescription, 
                         Org.tbAccount.InsertedBy, Org.tbAccount.InsertedOn, Org.tbAccount.LiquidityLevel
FROM            Org.tbOrg INNER JOIN
                         Org.tbAccount ON Org.tbOrg.AccountCode = Org.tbAccount.AccountCode INNER JOIN
                         Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                         Org.tbAccountType ON Org.tbAccount.AccountTypeCode = Org.tbAccountType.AccountTypeCode LEFT OUTER JOIN
                         Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode
