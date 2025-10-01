CREATE   VIEW Cash.vwChangeCollection
AS
	SELECT        change.PaymentAddress, Cash.fnChangeKeyPath(account.CoinTypeCode, account_key.HDPath.ToString(), change.ChangeTypeCode, change.AddressIndex)  FullHDPath
	FROM            Cash.tbChange AS change INNER JOIN
							 Org.tbAccountKey AS account_key ON change.CashAccountCode = account_key.CashAccountCode AND change.HDPath = account_key.HDPath INNER JOIN
							 Org.tbAccount AS account ON account_key.CashAccountCode = account.CashAccountCode
	WHERE        (change.ChangeStatusCode < 2);
