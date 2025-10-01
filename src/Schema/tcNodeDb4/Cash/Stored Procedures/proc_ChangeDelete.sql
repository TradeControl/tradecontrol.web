CREATE   PROCEDURE Cash.proc_ChangeDelete (@PaymentAddress nvarchar(42))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		IF EXISTS (
			SELECT * FROM Cash.tbChange change
				OUTER APPLY
				(
					SELECT PaymentAddress, SUM(MoneyIn) Balance
					FROM Cash.tbTx tx
					WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
					GROUP BY PaymentAddress			
				) change_balance
			WHERE change.PaymentAddress = @PaymentAddress AND ChangeStatusCode = 0 AND COALESCE(change_balance.Balance, 0) = 0)
		BEGIN
			DELETE FROM Cash.tbChangeReference WHERE PaymentAddress = @PaymentAddress;
			DELETE FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress;			
		END
		ELSE
			RETURN 1;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
