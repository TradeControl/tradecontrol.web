
CREATE   PROCEDURE Subject.proc_Statement (@SubjectCode NVARCHAR(10))
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT *
		FROM Subject.vwStatement
		WHERE SubjectCode = @SubjectCode
		ORDER BY RowNumber DESC

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
