CREATE   PROCEDURE Cash.proc_PaymentDelete
	(
	@PaymentCode nvarchar(20)
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@AccountCode nvarchar(10)
			, @CashAccountCode nvarchar(10)

		SELECT  @AccountCode = AccountCode, @CashAccountCode = CashAccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)

		DELETE FROM Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		EXEC Org.proc_Rebuild @AccountCode

		BEGIN TRANSACTION
		EXEC Cash.proc_AccountRebuild @CashAccountCode
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

