CREATE VIEW Cash.vwBankAccounts
AS
	SELECT AccountCode, AccountName, OpeningBalance, CASE WHEN NOT CashCode IS NULL THEN 0 ELSE 1 END AS DisplayOrder
	FROM Subject.tbAccount  
	WHERE (AccountTypeCode = 0)
