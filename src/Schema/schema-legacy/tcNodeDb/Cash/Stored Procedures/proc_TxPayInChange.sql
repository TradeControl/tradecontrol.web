CREATE   PROCEDURE Cash.proc_TxPayInChange
(
	@CashAccountCode nvarchar(10), 
	@PaymentAddress nvarchar(42),
	@TxId nvarchar(64),
	@AccountCode nvarchar(10), 
	@CashCode nvarchar(50),
	@PaymentReference nvarchar(50) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE 
			@PaymentCode nvarchar(20)
			, @TaxCode nvarchar(10) = (SELECT TaxCode FROM Cash.tbCode WHERE CashCode = @CashCode);
			
		BEGIN TRAN

		EXECUTE Cash.proc_NextPaymentCode  @PaymentCode OUTPUT

		INSERT INTO Cash.tbPayment (UserId, PaymentCode, CashAccountCode, PaidOn, AccountCode, PaymentStatusCode, PaidInValue, CashCode, TaxCode, PaymentReference)
		SELECT 
			(SELECT UserId FROM Usr.vwCredentials) UserId,
			@PaymentCode PaymentCode, @CashAccountCode CashAccountCode, CURRENT_TIMESTAMP PaidOn, @AccountCode AccountCode, 1 PaymentStatusCode, MoneyIn, 
			@CashCode CashCode, @TaxCode TaxCode, @PaymentReference PaymentReference
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		UPDATE  Org.tbAccount
		SET CurrentBalance = Org.tbAccount.CurrentBalance + PaidInValue
		FROM         Org.tbAccount INNER JOIN
							  Cash.tbPayment ON Org.tbAccount.CashAccountCode = Cash.tbPayment.CashAccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		UPDATE Cash.tbTx
		SET TxStatusCode = 1
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		INSERT INTO Cash.tbTxReference (TxNumber, PaymentCode, TxStatusCode)
		SELECT TxNumber, @PaymentCode PaymentCode, TxStatusCode
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
