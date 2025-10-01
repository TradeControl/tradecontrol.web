CREATE   FUNCTION Cash.fnTx(@CashAccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE
AS
	RETURN
	(
		WITH tx AS
		(
			SELECT        change.CashAccountCode, Org.tbAccount.CoinTypeCode, change.PaymentAddress, change.HDPath, change.ChangeTypeCode, change_type.ChangeType, change.ChangeStatusCode, change_status.ChangeStatus, 
									 change.AddressIndex, tx.TxId, tx.TransactedOn, tx.TxStatusCode, tx_status.TxStatus, tx.MoneyIn, tx.MoneyOut, tx.Confirmations, tx.TxMessage, tx.InsertedBy, tx_ref.PaymentCodeIn, tx_ref.PaymentCodeOut
			FROM            Cash.tbTx AS tx INNER JOIN
                         Cash.tbTxStatus AS tx_status ON tx.TxStatusCode = tx_status.TxStatusCode AND tx.TxStatusCode = tx_status.TxStatusCode INNER JOIN
                         Cash.tbChange AS change ON tx.PaymentAddress = change.PaymentAddress AND tx.PaymentAddress = change.PaymentAddress INNER JOIN
                         Cash.tbChangeType AS change_type ON change.ChangeTypeCode = change_type.ChangeTypeCode AND change.ChangeTypeCode = change_type.ChangeTypeCode INNER JOIN
                         Cash.tbChangeStatus AS change_status ON change.ChangeStatusCode = change_status.ChangeStatusCode INNER JOIN
                         Org.tbAccount ON change.CashAccountCode = Org.tbAccount.CashAccountCode
						 LEFT OUTER JOIN vwTxReference tx_ref ON tx.TxNumber = tx_ref.TxNumber
		), key_namespace AS
		(
			SELECT CashAccountCode, HDPath, KeyNamespace, KeyName
			FROM Org.fnKeyNamespace(@CashAccountCode, @KeyName) kn
		)
		SELECT tx.CashAccountCode, KeyNamespace, KeyName, PaymentAddress, ChangeTypeCode, ChangeType, ChangeStatusCode, ChangeStatus, 
			Cash.fnChangeKeyPath(tx.CoinTypeCode, key_namespace.HDPath.ToString(), tx.ChangeTypeCode, tx.AddressIndex)  FullHDPath,
			TxId, TransactedOn, TxStatusCode, TxStatus, MoneyIn, MoneyOut, Confirmations, TxMessage, InsertedBy, PaymentCodeIn, PaymentCodeOut
		FROM  key_namespace 
			JOIN tx ON key_namespace.HDPath = tx.HDPath
	)
