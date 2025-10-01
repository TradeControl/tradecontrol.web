CREATE   VIEW Cash.vwTxReference
AS
	WITH tx AS
	(
		SELECT TxNumber
		FROM Cash.tbTx
	), pay_in AS
	(
		SELECT TxNumber, PaymentCode PaymentCodeIn
		FROM Cash.tbTxReference
		WHERE TxStatusCode = 1
	), pay_out AS
	(
		SELECT TxNumber, PaymentCode PaymentCodeOut
		FROM Cash.tbTxReference
		WHERE TxStatusCode = 2
	)
	SELECT tx.TxNumber, PaymentCodeIn, PaymentCodeOut
	FROM tx 
		LEFT OUTER JOIN pay_in ON tx.TxNumber = pay_in.TxNumber
		LEFT OUTER JOIN pay_out ON tx.TxNumber = pay_out.TxNumber;
