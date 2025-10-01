CREATE   FUNCTION Cash.fnKeyNameBalance(@CashAccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS float
AS
BEGIN
	DECLARE @Balance float;

	SELECT @Balance = SUM(COALESCE(change_balance.Balance, 0))
	FROM Org.tbAccountKey accountKey
		JOIN Cash.tbChange change
			ON accountKey.HDPath = change.HDPath AND accountKey.CashAccountCode = change.CashAccountCode
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress			
		) change_balance
	WHERE accountKey.CashAccountCode = @CashAccountCode AND accountKey.KeyName = @KeyName;

	RETURN @Balance;
END
