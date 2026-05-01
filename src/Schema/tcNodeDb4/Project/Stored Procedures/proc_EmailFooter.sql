
CREATE PROCEDURE Project.proc_EmailFooter
AS
-- candidate for deletion
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT
			u.UserName,
			u.PhoneNumber,
			u.MobileNumber,
			o.SubjectName,
			o.WebSite
		FROM Usr.vwCredentials AS c
			INNER JOIN Usr.tbUser AS u
				ON c.UserId = u.UserId
			CROSS JOIN
			(
				SELECT TOP (1)
					s.SubjectName,
					v.WebSite
				FROM App.tbOptions AS opt
					INNER JOIN Subject.tbSubject AS s
						ON opt.SubjectCode = s.SubjectCode
					LEFT OUTER JOIN Subject.tbVirtual AS v
						ON s.SubjectCode = v.SubjectCode
			) AS o;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
GO
