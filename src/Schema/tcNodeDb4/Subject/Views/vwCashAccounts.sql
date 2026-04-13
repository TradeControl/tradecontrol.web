CREATE VIEW Subject.vwCashAccounts
AS
SELECT
	Subject.tbAccount.AccountCode,
	Subject.tbSubject.SubjectCode,
	Subject.tbAccount.AccountName,
	Subject.tbSubject.SubjectName,
	Subject.tbType.SubjectType,
	Subject.tbAccount.OpeningBalance,
	Subject.tbAccount.CurrentBalance,
	Subject.tbAccount.SortCode,
	Subject.tbAccount.AccountNumber,
	Subject.tbAccount.AccountClosed,
	Subject.tbAccount.AccountTypeCode,
	Subject.tbAccountType.AccountType,
	Subject.tbAccount.CashCode,
	Cash.tbCode.CashDescription,
	Subject.tbAccount.BalanceConstraintCode,
	Subject.tbBalanceConstraint.BalanceConstraint,
	Subject.tbAccount.InsertedBy,
	Subject.tbAccount.InsertedOn,
	Subject.tbAccount.LiquidityLevel
FROM Subject.tbSubject
	INNER JOIN Subject.tbAccount
		ON Subject.tbSubject.SubjectCode = Subject.tbAccount.SubjectCode
	INNER JOIN Subject.tbType
		ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
	INNER JOIN Subject.tbAccountType
		ON Subject.tbAccount.AccountTypeCode = Subject.tbAccountType.AccountTypeCode
	INNER JOIN Subject.tbBalanceConstraint
		ON Subject.tbAccount.BalanceConstraintCode = Subject.tbBalanceConstraint.BalanceConstraintCode
	LEFT OUTER JOIN Cash.tbCode
		ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode;
