CREATE   PROCEDURE Cash.proc_ChangeTxAdd(@PaymentAddress nvarchar(42), @TxId nvarchar(64), @TxStatusCode smallint, @MoneyIn decimal(18, 5), @Confirmations int, @TxMessage nvarchar(50) = null)
AS
	SET XACT_ABORT, NOCOUNT ON;
	BEGIN TRY

		BEGIN TRAN

		DECLARE @PaymentCode nvarchar(20);

		IF EXISTS (SELECT * FROM Cash.tbTx WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress)
		BEGIN
			UPDATE Cash.tbTx
			SET 
				MoneyIn = @MoneyIn, 
				TxStatusCode = CASE WHEN @TxStatusCode = 2 THEN @TxStatusCode ELSE TxStatusCode END,
				Confirmations = @Confirmations
			WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;
		END
		ELSE
		BEGIN
			SELECT @TxStatusCode = CASE change.ChangeTypeCode WHEN 1 THEN 1 ELSE @TxStatusCode END
			FROM Cash.tbChange change
				JOIN Cash.tbTx tx ON change.PaymentAddress = tx.PaymentAddress
			WHERE tx.PaymentAddress = @PaymentAddress AND tx.TxId = @TxId;

			INSERT INTO Cash.tbTx (TxId, PaymentAddress, TxStatusCode, MoneyIn, Confirmations, TxMessage)
			VALUES (@TxId, @PaymentAddress, @TxStatusCode, @MoneyIn, @Confirmations, @TxMessage);
		END

		EXEC Cash.proc_TxInvoice @PaymentAddress, @TxId;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

