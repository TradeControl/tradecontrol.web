CREATE   PROCEDURE Cash.proc_TxPayIn
(
	@AccountCode nvarchar(10), 
	@PaymentAddress nvarchar(42),
	@TxId nvarchar(64),
	@SubjectCode nvarchar(10), 
	@CashCode nvarchar(50), 
	@PaidOn datetime, 
	@PaymentReference nvarchar(50) = null, 
	@PaymentCode nvarchar(20) output)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE @ToPay decimal(18, 5), @Confirmations int;

		SELECT @ToPay = MoneyIn - MoneyOut, @Confirmations = Confirmations 
		FROM Cash.tbTx 
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress

		IF NOT EXISTS (SELECT * FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode)
			SELECT @SubjectCode = SubjectCode FROM App.vwHomeAccount;
		ELSE IF @Confirmations = 0 
			RETURN 1;

		BEGIN TRAN

		EXEC Cash.proc_PaymentAdd @SubjectCode, @AccountCode, @CashCode, @PaidOn, @ToPay, @PaymentReference, @PaymentCode OUTPUT;

		UPDATE Cash.tbTx
		SET TxStatusCode = 1
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		INSERT INTO Cash.tbTxReference (TxNumber, PaymentCode, TxStatusCode)
		SELECT TxNumber, @PaymentCode PaymentCode, TxStatusCode
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		IF EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode AND PaymentStatusCode = 2)
			EXEC Cash.proc_PayAccrual @PaymentCode;
		ELSE
			EXEC Cash.proc_PaymentPost;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
