
CREATE   PROCEDURE Cash.proc_AccountRebuild
	(
	@CashAccountCode nvarchar(10)
	)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		UPDATE Org.tbAccount
		SET CurrentBalance = Cash.vwAccountRebuild.CurrentBalance
		FROM         Cash.vwAccountRebuild INNER JOIN
							Org.tbAccount ON Cash.vwAccountRebuild.CashAccountCode = Org.tbAccount.CashAccountCode
		WHERE Cash.vwAccountRebuild.CashAccountCode = @CashAccountCode 

		UPDATE Org.tbAccount
		SET CurrentBalance = 0
		FROM         Cash.vwAccountRebuild RIGHT OUTER JOIN
							  Org.tbAccount ON Cash.vwAccountRebuild.CashAccountCode = Org.tbAccount.CashAccountCode
		WHERE     (Cash.vwAccountRebuild.CashAccountCode IS NULL) AND Org.tbAccount.CashAccountCode = @CashAccountCode
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH 
