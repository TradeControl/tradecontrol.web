CREATE VIEW Cash.vwBankAccounts
AS
	SELECT CashAccountCode, CashAccountName, OpeningBalance, CASE WHEN NOT CashCode IS NULL THEN 0 ELSE 1 END AS DisplayOrder
	FROM Org.tbAccount  
	WHERE (AccountTypeCode = 0)
