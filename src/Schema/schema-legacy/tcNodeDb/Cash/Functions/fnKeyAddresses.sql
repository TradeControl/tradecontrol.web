CREATE   FUNCTION Cash.fnKeyAddresses(@CashAccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE
AS
	RETURN
	(
		SELECT        
			Cash.fnChangeKeyPath(cash_account.CoinTypeCode, key_name.HDPath.ToString(), change.ChangeTypeCode, AddressIndex)  HDPath, 
			change.PaymentAddress, change.AddressIndex
		FROM Cash.tbChange AS change 
			INNER JOIN Org.tbAccountKey AS key_name 
				ON change.CashAccountCode = key_name.CashAccountCode AND change.HDPath = key_name.HDPath AND change.CashAccountCode = key_name.CashAccountCode AND change.HDPath = key_name.HDPath 
			INNER JOIN Org.tbAccount AS cash_account 
				ON key_name.CashAccountCode = cash_account.CashAccountCode
		WHERE (change.ChangeStatusCode = 1) AND (key_name.CashAccountCode = @CashAccountCode) AND (key_name.KeyName = @KeyName)
	)
