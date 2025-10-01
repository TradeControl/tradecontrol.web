CREATE   PROCEDURE Invoice.proc_DefaultPaymentOn
	(
		@AccountCode nvarchar(10),
		@ActionOn datetime,
		@PaymentOn datetime output
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT @ActionOn = CASE WHEN org.PayDaysFromMonthEnd <> 0 
				THEN 
					DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, org.PaymentDays, @ActionOn), 'yyyyMM'), '01')))												
				ELSE
					DATEADD(d, org.PaymentDays, @ActionOn)	
				END
		FROM Org.tbOrg org 
		WHERE org.AccountCode = @AccountCode

		SELECT @PaymentOn = App.fnAdjustToCalendar(@ActionOn, 0) 					
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
