
CREATE   PROCEDURE Task.proc_EmailDetail 
	(
	@TaskCode nvarchar(20)
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@NickName nvarchar(100)
			, @EmailAddress nvarchar(255)

		IF EXISTS(SELECT     Org.tbContact.ContactName
				  FROM         Org.tbContact INNER JOIN
										Task.tbTask ON Org.tbContact.AccountCode = Task.tbTask.AccountCode AND Org.tbContact.ContactName = Task.tbTask.ContactName
				  WHERE     ( Task.tbTask.TaskCode = @TaskCode))
			BEGIN
			SELECT  @NickName = CASE WHEN Org.tbContact.NickName is null THEN Org.tbContact.ContactName ELSE Org.tbContact.NickName END
						  FROM         Org.tbContact INNER JOIN
												tbTask ON Org.tbContact.AccountCode = Task.tbTask.AccountCode AND Org.tbContact.ContactName = Task.tbTask.ContactName
						  WHERE     ( Task.tbTask.TaskCode = @TaskCode)				
			END
		ELSE
			BEGIN
			SELECT @NickName = ContactName
			FROM         Task.tbTask
			WHERE     (TaskCode = @TaskCode)
			END
	
		EXEC Task.proc_EmailAddress	@TaskCode, @EmailAddress output
	
		SELECT     Task.tbTask.TaskCode, Task.tbTask.TaskTitle, Org.tbOrg.AccountCode, Org.tbOrg.AccountName, @NickName AS NickName, @EmailAddress AS EmailAddress, 
							  Task.tbTask.ActivityCode, Task.tbStatus.TaskStatus, Task.tbTask.TaskNotes
		FROM         Task.tbTask INNER JOIN
							  Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
							  Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode
		WHERE     ( Task.tbTask.TaskCode = @TaskCode)

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
