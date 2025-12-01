CREATE   FUNCTION Project.fnEmailAddress
	(
	@ProjectCode nvarchar(20)
	)
RETURNS nvarchar(255)
AS
	BEGIN
	DECLARE @EmailAddress nvarchar(255)

	IF EXISTS(SELECT     Subject.tbContact.EmailAddress
		  FROM         Subject.tbContact INNER JOIN
								tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
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
	
	RETURN @EmailAddress
	END

