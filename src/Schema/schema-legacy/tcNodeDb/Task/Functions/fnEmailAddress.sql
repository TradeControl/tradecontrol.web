CREATE   FUNCTION Task.fnEmailAddress
	(
	@TaskCode nvarchar(20)
	)
RETURNS nvarchar(255)
AS
	BEGIN
	DECLARE @EmailAddress nvarchar(255)

	IF EXISTS(SELECT     Org.tbContact.EmailAddress
		  FROM         Org.tbContact INNER JOIN
								tbTask ON Org.tbContact.AccountCode = Task.tbTask.AccountCode AND Org.tbContact.ContactName = Task.tbTask.ContactName
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
	
	RETURN @EmailAddress
	END

