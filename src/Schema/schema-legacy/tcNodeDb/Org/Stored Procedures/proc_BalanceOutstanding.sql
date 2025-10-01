CREATE PROCEDURE Org.proc_BalanceOutstanding 
	(
	@AccountCode nvarchar(10),
	@Balance decimal(18, 5) = 0 OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY		
		
		SELECT @Balance = ISNULL(Balance, 0) FROM Org.vwBalanceOutstanding WHERE AccountCode = @AccountCode

		IF EXISTS(SELECT     AccountCode
				  FROM         Cash.tbPayment
				  WHERE     (PaymentStatusCode = 0) AND (AccountCode = @AccountCode)) AND (@Balance <> 0)
			BEGIN
			SELECT  @Balance = @Balance - SUM(PaidInValue - PaidOutValue) 
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (AccountCode = @AccountCode)		
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

