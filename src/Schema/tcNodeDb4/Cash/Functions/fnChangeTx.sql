CREATE   FUNCTION Cash.fnChangeTx(@PaymentAddress nvarchar(42))
RETURNS TABLE
AS
	RETURN
	(
		SELECT Cash.tbTx.PaymentAddress, Cash.tbTx.TxId, Cash.tbTx.TransactedOn, Cash.tbTx.TxStatusCode, Cash.tbTxStatus.TxStatus, Cash.tbTx.MoneyIn, Cash.tbTx.MoneyOut, Cash.tbTx.Confirmations, Cash.tbTx.InsertedBy, payments.PaymentCodeIn, payments.PaymentCodeOut, Cash.tbTx.TxMessage
		FROM Cash.tbTx 
			INNER JOIN Cash.tbTxStatus ON Cash.tbTx.TxStatusCode = Cash.tbTxStatus.TxStatusCode 
			LEFT OUTER JOIN Cash.vwTxReference payments ON Cash.tbTx.TxNumber = payments.TxNumber
		WHERE        (Cash.tbTx.PaymentAddress = @PaymentAddress)		
	)
