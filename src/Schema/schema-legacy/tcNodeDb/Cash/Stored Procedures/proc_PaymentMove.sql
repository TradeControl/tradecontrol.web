CREATE   PROCEDURE Cash.proc_PaymentMove
	(
	@PaymentCode nvarchar(20),
	@CashAccountCode nvarchar(10)
	)
  AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @OldAccountCode nvarchar(10)

		SELECT @OldAccountCode = CashAccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		BEGIN TRANSACTION
	
		UPDATE Cash.tbPayment 
		SET CashAccountCode = @CashAccountCode,
			UpdatedOn = CURRENT_TIMESTAMP,
			UpdatedBy = (suser_sname())
		WHERE PaymentCode = @PaymentCode	

		EXEC Cash.proc_AccountRebuild @CashAccountCode
		EXEC Cash.proc_AccountRebuild @OldAccountCode
	
		COMMIT TRANSACTION
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

