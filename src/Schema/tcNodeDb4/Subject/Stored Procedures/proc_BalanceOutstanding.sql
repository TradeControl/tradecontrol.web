CREATE PROCEDURE Subject.proc_BalanceOutstanding 
	(
	@SubjectCode nvarchar(10),
	@Balance decimal(18, 5) = 0 OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY		
		
		SELECT @Balance = ISNULL(Balance, 0) FROM Subject.vwBalanceOutstanding WHERE SubjectCode = @SubjectCode

		IF EXISTS(SELECT     SubjectCode
				  FROM         Cash.tbPayment
				  WHERE     (PaymentStatusCode = 0) AND (SubjectCode = @SubjectCode)) AND (@Balance <> 0)
			BEGIN
			SELECT  @Balance = @Balance - SUM(PaidInValue - PaidOutValue) 
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (SubjectCode = @SubjectCode)		
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

