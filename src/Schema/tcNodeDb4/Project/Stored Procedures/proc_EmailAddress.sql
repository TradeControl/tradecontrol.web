
CREATE   PROCEDURE Project.proc_EmailAddress 
	(
	@ProjectCode nvarchar(20),
	@EmailAddress nvarchar(255) OUTPUT
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     Subject.tbContact.EmailAddress
				  FROM         Subject.tbContact INNER JOIN
										Project.tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
				  WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
				  GROUP BY Subject.tbContact.EmailAddress
				  HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL)))
			BEGIN
			SELECT    @EmailAddress = Subject.tbContact.EmailAddress
			FROM         Subject.tbContact INNER JOIN
								tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
			WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
			GROUP BY Subject.tbContact.EmailAddress
			HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL))	
			END
		ELSE
			BEGIN
			SELECT    @EmailAddress =  Subject.tbSubject.EmailAddress
			FROM         Subject.tbSubject INNER JOIN
								 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
			WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
