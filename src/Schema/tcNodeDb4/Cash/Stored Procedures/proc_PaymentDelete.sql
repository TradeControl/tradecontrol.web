CREATE   PROCEDURE Cash.proc_PaymentDelete
	(
	@PaymentCode nvarchar(20)
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@SubjectCode nvarchar(10)
			, @AccountCode nvarchar(10)

		SELECT  @SubjectCode = SubjectCode, @AccountCode = AccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)

		DELETE FROM Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		EXEC Subject.proc_Rebuild @SubjectCode

		BEGIN TRANSACTION
		EXEC Cash.proc_AccountRebuild @AccountCode
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

