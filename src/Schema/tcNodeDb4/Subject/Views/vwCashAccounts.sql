CREATE VIEW Subject.vwCashAccounts
AS
SELECT        Subject.tbAccount.AccountCode, Subject.tbSubject.SubjectCode, Subject.tbAccount.AccountName, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbAccount.OpeningBalance, Subject.tbAccount.CurrentBalance, 
                         Subject.tbAccount.SortCode, Subject.tbAccount.AccountNumber, Subject.tbAccount.AccountClosed, Subject.tbAccount.AccountTypeCode, Subject.tbAccountType.AccountType, Subject.tbAccount.CashCode, Cash.tbCode.CashDescription, 
                         Subject.tbAccount.InsertedBy, Subject.tbAccount.InsertedOn, Subject.tbAccount.LiquidityLevel
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbAccount ON Subject.tbSubject.SubjectCode = Subject.tbAccount.SubjectCode INNER JOIN
                         Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                         Subject.tbAccountType ON Subject.tbAccount.AccountTypeCode = Subject.tbAccountType.AccountTypeCode LEFT OUTER JOIN
                         Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode AND Subject.tbAccount.CashCode = Cash.tbCode.CashCode AND Subject.tbAccount.CashCode = Cash.tbCode.CashCode
