CREATE   FUNCTION Cash.fnChangeUnassigned (@CashAccountCode nvarchar(10))
RETURNS TABLE
AS
	RETURN
	(
		SELECT change.CashAccountCode, Org.fnAccountKeyNamespace(account_key.CashAccountCode, account_key.HDPath) AS KeyNamespace, 
			account_key.KeyName, change.PaymentAddress, change.Note, change.InsertedOn, change.UpdatedOn, COALESCE(change_balance.Balance, 0) Balance
		FROM Cash.tbChange AS change 
				OUTER APPLY
				(
					SELECT PaymentAddress, SUM(MoneyIn) Balance
					FROM Cash.tbTx tx
					WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
					GROUP BY PaymentAddress			
				) change_balance
			JOIN Org.tbAccountKey account_key ON change.CashAccountCode = account_key.CashAccountCode AND change.HDPath = account_key.HDPath
			LEFT OUTER JOIN Cash.tbChangeReference ON change.PaymentAddress = Cash.tbChangeReference.PaymentAddress
		WHERE  (change.CashAccountCode = @CashAccountCode)  AND (change.ChangeTypeCode = 0) AND (Cash.tbChangeReference.PaymentAddress IS NULL) AND (change.ChangeStatusCode = 0)
	)
