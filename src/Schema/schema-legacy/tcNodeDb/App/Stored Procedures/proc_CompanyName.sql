
CREATE   PROCEDURE App.proc_CompanyName
	(
	@AccountName nvarchar(255) = null output
	)
  AS
	SELECT TOP 1 @AccountName = Org.tbOrg.AccountName
	FROM         Org.tbOrg INNER JOIN
	                      App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode
	 
