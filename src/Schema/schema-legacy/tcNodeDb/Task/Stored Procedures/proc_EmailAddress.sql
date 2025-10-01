
CREATE   PROCEDURE Task.proc_EmailAddress 
	(
	@TaskCode nvarchar(20),
	@EmailAddress nvarchar(255) OUTPUT
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     Org.tbContact.EmailAddress
				  FROM         Org.tbContact INNER JOIN
										Task.tbTask ON Org.tbContact.AccountCode = Task.tbTask.AccountCode AND Org.tbContact.ContactName = Task.tbTask.ContactName
				  WHERE     ( Task.tbTask.TaskCode = @TaskCode)
				  GROUP BY Org.tbContact.EmailAddress
				  HAVING      (NOT ( Org.tbContact.EmailAddress IS NULL)))
			BEGIN
			SELECT    @EmailAddress = Org.tbContact.EmailAddress
			FROM         Org.tbContact INNER JOIN
								tbTask ON Org.tbContact.AccountCode = Task.tbTask.AccountCode AND Org.tbContact.ContactName = Task.tbTask.ContactName
			WHERE     ( Task.tbTask.TaskCode = @TaskCode)
			GROUP BY Org.tbContact.EmailAddress
			HAVING      (NOT ( Org.tbContact.EmailAddress IS NULL))	
			END
		ELSE
			BEGIN
			SELECT    @EmailAddress =  Org.tbOrg.EmailAddress
			FROM         Org.tbOrg INNER JOIN
								 Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode
			WHERE     ( Task.tbTask.TaskCode = @TaskCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
