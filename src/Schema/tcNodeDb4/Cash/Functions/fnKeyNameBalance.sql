CREATE   FUNCTION Cash.fnKeyNameBalance(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS float
AS
BEGIN
	DECLARE @Balance float;

	SELECT @Balance = SUM(COALESCE(change_balance.Balance, 0))
	FROM Subject.tbAccountKey accountKey
		JOIN Cash.tbChange change
			ON accountKey.HDPath = change.HDPath AND accountKey.AccountCode = change.AccountCode
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress			
		) change_balance
	WHERE accountKey.AccountCode = @AccountCode AND accountKey.KeyName = @KeyName;

	RETURN @Balance;
END
