CREATE VIEW Org.vwCashAccountAssets
AS
	SELECT        Org.tbAccount.CashAccountCode, Org.tbAccount.LiquidityLevel, Org.tbAccount.CashAccountName, Org.tbAccount.AccountCode, Cash.tbCode.CashCode, Cash.tbCode.TaxCode, Org.tbAccount.AccountClosed
	FROM            Org.tbAccount INNER JOIN
							 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Org.tbAccount.AccountTypeCode = 2);
