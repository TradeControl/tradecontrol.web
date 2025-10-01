
CREATE   PROCEDURE Cash.proc_AccountRebuild
	(
	@AccountCode nvarchar(10)
	)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		UPDATE Subject.tbAccount
		SET CurrentBalance = Cash.vwAccountRebuild.CurrentBalance
		FROM         Cash.vwAccountRebuild INNER JOIN
							Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE Cash.vwAccountRebuild.AccountCode = @AccountCode 

		UPDATE Subject.tbAccount
		SET CurrentBalance = 0
		FROM         Cash.vwAccountRebuild RIGHT OUTER JOIN
							  Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE     (Cash.vwAccountRebuild.AccountCode IS NULL) AND Subject.tbAccount.AccountCode = @AccountCode
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH 
