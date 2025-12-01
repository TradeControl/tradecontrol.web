CREATE   VIEW Cash.vwChangeCollection
AS
	SELECT        change.PaymentAddress, Cash.fnChangeKeyPath(account.CoinTypeCode, account_key.HDPath.ToString(), change.ChangeTypeCode, change.AddressIndex)  FullHDPath
	FROM            Cash.tbChange AS change INNER JOIN
							 Subject.tbAccountKey AS account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath INNER JOIN
							 Subject.tbAccount AS account ON account_key.AccountCode = account.AccountCode
	WHERE        (change.ChangeStatusCode < 2);
