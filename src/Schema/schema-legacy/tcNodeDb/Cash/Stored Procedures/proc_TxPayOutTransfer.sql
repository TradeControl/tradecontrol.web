CREATE   PROCEDURE Cash.proc_TxPayOutTransfer
(
	@PaymentAddress nvarchar(42)
	, @TxId nvarchar(64)
	, @Spent decimal(18, 5)
	, @MinerFee decimal(18, 5)
	, @CashCode nvarchar(50)
	, @PaymentReference nvarchar(50)
)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 						
			@AccountCode nvarchar(10)
			, @TaxCode nvarchar(10)
			, @PaymentCode nvarchar(20)
			, @TxNumber int
			, @PaidOutValue decimal(18, 5)
			, @CashAccountCode nvarchar(10) = (SELECT CashAccountCode FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress)
			, @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials)
			;

		SELECT @TxNumber = TxNumber, @PaidOutValue = MoneyIn - @MinerFee
		FROM Cash.tbTx 
		WHERE PaymentAddress = @PaymentAddress AND TxId = @TxId;

		SELECT @AccountCode = AccountCode FROM App.vwHomeAccount;
		SELECT @TaxCode = TaxCode FROM Cash.tbCode WHERE CashCode = @CashCode;

		BEGIN TRAN

		UPDATE Cash.tbTx
		SET
			TxStatusCode = 2, 
			MoneyOut = @Spent
		WHERE PaymentAddress = @PaymentAddress AND TxNumber = @TxNumber;

		EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, AccountCode, CashAccountCode, CashCode, TaxCode, PaidOutValue, PaymentReference)
		VALUES (@PaymentCode, @UserId, CURRENT_TIMESTAMP, @AccountCode, @CashAccountCode, @CashCode, @TaxCode, @PaidOutValue, @PaymentReference);

		IF EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode AND PaymentStatusCode = 2)
			EXEC Cash.proc_PayAccrual @PaymentCode;
		ELSE
			EXEC Cash.proc_PaymentPost;

		INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
		VALUES (@TxNumber, 2, @PaymentCode);

		IF @MinerFee > 0
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, PaymentStatusCode, AccountCode, CashAccountCode, CashCode, TaxCode, PaidOutValue, PaymentReference)
			SELECT @PaymentCode PaymentCode, @UserId UserId, CURRENT_TIMESTAMP PaidOn, 0 PaymentStatusCode, options.MinerAccountCode AccountCode, @CashAccountCode CashAccountCode,
				cash_code.CashCode CashCode, cash_code.TaxCode TaxCode, @MinerFee PaidOutValue, @PaymentReference PaymentReference
			FROM App.tbOptions options
				JOIN Cash.tbCode cash_code ON options.MinerFeeCode = cash_code.CashCode;	

			EXEC Cash.proc_PaymentPost;
		END
		
		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
