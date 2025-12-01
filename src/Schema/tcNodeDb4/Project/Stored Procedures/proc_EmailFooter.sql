
CREATE   PROCEDURE Project.proc_EmailFooter 
AS
--mod replace with view

	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT        u.UserName, u.PhoneNumber, u.MobileNumber, o.SubjectName, o.WebSite
		FROM            Usr.vwCredentials AS c INNER JOIN
								 Usr.tbUser AS u ON c.UserId = u.UserId 
			CROSS JOIN
			(SELECT        TOP (1) Subject.tbSubject.SubjectName, Subject.tbSubject.WebSite
			FROM            Subject.tbSubject INNER JOIN
										App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode) AS o

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
