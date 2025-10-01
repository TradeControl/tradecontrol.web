CREATE FUNCTION Cash.fnChangeUnassigned (@AccountCode nvarchar(10))
RETURNS TABLE
AS
	RETURN
	(
		SELECT change.AccountCode, Subject.fnAccountKeyNamespace(account_key.AccountCode, account_key.HDPath) AS KeyNamespace, 
			account_key.KeyName, change.PaymentAddress, change.Note, change.InsertedOn, change.UpdatedOn, COALESCE(change_balance.Balance, 0) Balance
		FROM Cash.tbChange AS change 
				OUTER APPLY
				(
					SELECT PaymentAddress, SUM(MoneyIn) Balance
					FROM Cash.tbTx tx
					WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
					GROUP BY PaymentAddress			
				) change_balance
			JOIN Subject.tbAccountKey account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath
			LEFT OUTER JOIN Cash.tbChangeReference ON change.PaymentAddress = Cash.tbChangeReference.PaymentAddress
		WHERE  (change.AccountCode = @AccountCode)  AND (change.ChangeTypeCode = 0) AND (Cash.tbChangeReference.PaymentAddress IS NULL) AND (change.ChangeStatusCode = 0)
	)
