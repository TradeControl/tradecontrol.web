CREATE   FUNCTION Cash.fnKeyAddresses(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE
AS
	RETURN
	(
		SELECT        
			Cash.fnChangeKeyPath(cash_account.CoinTypeCode, key_name.HDPath.ToString(), change.ChangeTypeCode, AddressIndex)  HDPath, 
			change.PaymentAddress, change.AddressIndex
		FROM Cash.tbChange AS change 
			INNER JOIN Subject.tbAccountKey AS key_name 
				ON change.AccountCode = key_name.AccountCode AND change.HDPath = key_name.HDPath AND change.AccountCode = key_name.AccountCode AND change.HDPath = key_name.HDPath 
			INNER JOIN Subject.tbAccount AS cash_account 
				ON key_name.AccountCode = cash_account.AccountCode
		WHERE (change.ChangeStatusCode = 1) AND (key_name.AccountCode = @AccountCode) AND (key_name.KeyName = @KeyName)
	)
