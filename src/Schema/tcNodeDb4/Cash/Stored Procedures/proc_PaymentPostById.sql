CREATE   PROCEDURE Cash.proc_PaymentPostById(@UserId nvarchar(10)) 
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PaymentCode nvarchar(20)

		DECLARE curMisc cursor local for
			SELECT        Cash.tbPayment.PaymentCode
			FROM            Cash.tbPayment 
				INNER JOIN Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode 
				INNER JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
				INNER JOIN Subject.tbAccount ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
			WHERE (Subject.tbAccount.AccountTypeCode < 2)
				AND (Cash.tbPayment.PaymentStatusCode = 0) 
				AND (Cash.tbPayment.UserId = @UserId)

			ORDER BY Cash.tbPayment.SubjectCode, Cash.tbPayment.PaidOn

		DECLARE curInv cursor local for
			SELECT     PaymentCode
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (CashCode IS NULL)
				AND (Cash.tbPayment.UserId = @UserId)
			ORDER BY SubjectCode, PaidOn
		
		BEGIN TRANSACTION

		OPEN curMisc
		FETCH NEXT FROM curMisc INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostMisc @PaymentCode		
			FETCH NEXT FROM curMisc INTO @PaymentCode	
			END

		CLOSE curMisc
		DEALLOCATE curMisc
	
		OPEN curInv
		FETCH NEXT FROM curInv INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostInvoiced @PaymentCode		
			FETCH NEXT FROM curInv INTO @PaymentCode	
			END

		CLOSE curInv
		DEALLOCATE curInv

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

