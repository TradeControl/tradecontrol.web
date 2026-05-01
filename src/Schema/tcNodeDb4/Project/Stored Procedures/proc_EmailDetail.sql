
CREATE PROCEDURE Project.proc_EmailDetail 
	(
	@ProjectCode nvarchar(20)
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@NickName nvarchar(100)
			, @EmailAddress nvarchar(255)

		IF EXISTS
        (
            SELECT Subject.vwReal.ContactName
			FROM Subject.vwReal
            INNER JOIN Project.tbProject
                ON Subject.vwReal.SubjectCode = Project.tbProject.SubjectCode AND Subject.vwReal.ContactName = Project.tbProject.ContactName
			WHERE(Project.tbProject.ProjectCode = @ProjectCode)
        )
		BEGIN
			SELECT @NickName = CASE WHEN Subject.vwReal.NickName is null THEN Subject.vwReal.ContactName ELSE Subject.vwReal.NickName END
			FROM Subject.vwReal
                INNER JOIN tbProject
                    ON Subject.vwReal.SubjectCode = Project.tbProject.SubjectCode AND Subject.vwReal.ContactName = Project.tbProject.ContactName
			WHERE Project.tbProject.ProjectCode = @ProjectCode
		END
		ELSE
		BEGIN
			SELECT @NickName = ContactName
			FROM         Project.tbProject
			WHERE     (ProjectCode = @ProjectCode)
		END
	
		EXEC Project.proc_EmailAddress	@ProjectCode, @EmailAddress output
	
		SELECT Project.tbProject.ProjectCode, Project.tbProject.ProjectTitle, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, @NickName AS NickName, @EmailAddress AS EmailAddress, 
				Project.tbProject.ObjectCode, Project.tbStatus.ProjectStatus, Project.tbProject.ProjectNotes
		FROM Project.tbProject
            INNER JOIN Project.tbStatus
                ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode
            INNER JOIN Subject.tbSubject
                ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE Project.tbProject.ProjectCode = @ProjectCode

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
