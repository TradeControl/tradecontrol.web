CREATE   PROCEDURE Cash.proc_TxPayOutMisc
(
	@PaymentAddress nvarchar(42)
	, @TxId nvarchar(64)
	, @Spent decimal(18, 5)
	, @MinerFee decimal(18, 5)
	, @SubjectCode nvarchar(10)
	, @CashCode nvarchar(50)
	, @TaxCode nvarchar(10)
	, @PaymentReference nvarchar(50)
)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 						
			@PaymentCode nvarchar(20)
			, @TxNumber int
			, @PaidOutValue decimal(18, 5)
			, @AccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress)
			, @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials)
			;

		SELECT @TxNumber = TxNumber, @PaidOutValue = MoneyIn - @MinerFee
		FROM Cash.tbTx 
		WHERE PaymentAddress = @PaymentAddress AND TxId = @TxId;

		BEGIN TRAN

		UPDATE Cash.tbTx
		SET
			TxStatusCode = 2, 
			MoneyOut = @Spent
		WHERE PaymentAddress = @PaymentAddress AND TxNumber = @TxNumber;

		EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue, PaymentStatusCode)
		VALUES (@PaymentCode, @UserId, CURRENT_TIMESTAMP, @SubjectCode, @AccountCode, @CashCode, @TaxCode, @PaidOutValue, 1);

		UPDATE  Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.CurrentBalance - PaidOutValue
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
		VALUES (@TxNumber, 2, @PaymentCode);

		IF @MinerFee > 0
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue)
			SELECT @PaymentCode PaymentCode, @UserId UserId, CURRENT_TIMESTAMP PaidOn, 0 PaymentStatusCode, options.MinerAccountCode SubjectCode, @AccountCode AccountCode,
				cash_code.CashCode CashCode, cash_code.TaxCode TaxCode, @MinerFee PaidOutValue
			FROM App.tbOptions options
				JOIN Cash.tbCode cash_code ON options.MinerFeeCode = cash_code.CashCode;				

			EXEC Cash.proc_PaymentPost;
		END
		
		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
