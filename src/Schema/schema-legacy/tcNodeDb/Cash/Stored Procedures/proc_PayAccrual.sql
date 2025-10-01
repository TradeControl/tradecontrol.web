CREATE PROCEDURE Cash.proc_PayAccrual (@PaymentCode NVARCHAR(20))
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		IF EXISTS (	SELECT        *
					FROM            Cash.tbPayment 
					WHERE        (PaymentStatusCode = 2) 
						AND UserId = (SELECT UserId FROM Usr.vwCredentials))
			BEGIN

			BEGIN TRANSACTION
			EXEC Cash.proc_PaymentPostMisc @PaymentCode	
			COMMIT TRANSACTION
			
			END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

