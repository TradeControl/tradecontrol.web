CREATE   FUNCTION Cash.fnNamespaceBalance(@CashAccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS float
AS
BEGIN
	DECLARE @Balance float;

	SELECT @Balance = SUM(COALESCE(change_balance.Balance, 0))
	FROM Org.fnKeyNamespace(@CashAccountCode, @KeyName) kn
		JOIN Cash.tbChange change
			ON kn.CashAccountCode = change.CashAccountCode AND kn.HDPath = change.HDPath
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress			
		) change_balance

	RETURN @Balance;
END
