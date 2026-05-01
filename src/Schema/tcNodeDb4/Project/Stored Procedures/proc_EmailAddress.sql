
CREATE   PROCEDURE Project.proc_EmailAddress 
	(
	@ProjectCode nvarchar(20),
	@EmailAddress nvarchar(255) OUTPUT
	)
AS
SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY
	IF EXISTS
    (
        SELECT Subject.vwReal.EmailAddress
		FROM Subject.vwReal
            INNER JOIN Project.tbProject
            ON Subject.vwReal.SubjectCode = Project.tbProject.SubjectCode AND Subject.vwReal.ContactName = Project.tbProject.ContactName
		WHERE Project.tbProject.ProjectCode = @ProjectCode
		GROUP BY Subject.vwReal.EmailAddress
		HAVING  (NOT ( Subject.vwReal.EmailAddress IS NULL))
    )
	BEGIN
		SELECT @EmailAddress = Subject.vwReal.EmailAddress
		FROM Subject.vwReal
            INNER JOIN tbProject
                ON Subject.vwReal.SubjectCode = Project.tbProject.SubjectCode AND Subject.vwReal.ContactName = Project.tbProject.ContactName
		WHERE Project.tbProject.ProjectCode = @ProjectCode
		GROUP BY Subject.vwReal.EmailAddress
		HAVING (NOT ( Subject.vwReal.EmailAddress IS NULL))	
	END
	ELSE
	BEGIN
		SELECT @EmailAddress =  Subject.tbSubject.EmailAddress
		FROM Subject.tbSubject
            INNER JOIN Project.tbProject
                ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
		WHERE Project.tbProject.ProjectCode = @ProjectCode
	END
		
END TRY
BEGIN CATCH
	EXEC App.proc_ErrorLog;
END CATCH
