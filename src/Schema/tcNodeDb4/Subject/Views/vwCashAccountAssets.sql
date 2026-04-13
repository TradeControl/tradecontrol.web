CREATE VIEW Subject.vwCashAccountAssets
AS
	SELECT
		Subject.tbAccount.AccountCode,
		Subject.tbAccount.LiquidityLevel,
		Subject.tbAccount.AccountName,
		Subject.tbAccount.SubjectCode,
		Cash.tbCode.CashCode,
		Cash.tbCode.TaxCode,
		Subject.tbAccount.AccountClosed,
		Subject.tbAccount.BalanceConstraintCode,
		Subject.tbBalanceConstraint.BalanceConstraint
	FROM Subject.tbAccount
		INNER JOIN Cash.tbCode
			ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
		INNER JOIN Subject.tbBalanceConstraint
			ON Subject.tbAccount.BalanceConstraintCode = Subject.tbBalanceConstraint.BalanceConstraintCode
	WHERE (Subject.tbAccount.AccountTypeCode = 2);
