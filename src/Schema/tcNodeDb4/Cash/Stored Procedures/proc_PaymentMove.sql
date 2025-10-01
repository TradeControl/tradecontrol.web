CREATE   PROCEDURE Cash.proc_PaymentMove
	(
	@PaymentCode nvarchar(20),
	@AccountCode nvarchar(10)
	)
  AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @OldAccountCode nvarchar(10)

		SELECT @OldAccountCode = AccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		BEGIN TRANSACTION
	
		UPDATE Cash.tbPayment 
		SET AccountCode = @AccountCode,
			UpdatedOn = CURRENT_TIMESTAMP,
			UpdatedBy = (suser_sname())
		WHERE PaymentCode = @PaymentCode	

		EXEC Cash.proc_AccountRebuild @AccountCode
		EXEC Cash.proc_AccountRebuild @OldAccountCode
	
		COMMIT TRANSACTION
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

