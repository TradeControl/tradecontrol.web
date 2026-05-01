CREATE PROCEDURE Subject.proc_NextAddressCode 
	(
	@SubjectCode nvarchar(50),
	@AddressCode nvarchar(15) OUTPUT
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@SubjectName nvarchar(100),
			@GeneratedCode nvarchar(50),
			@CheckSql nvarchar(max) = N'SELECT @cnt = COUNT(*) FROM Subject.tbAddress WHERE AddressCode = @Code';

		SELECT @SubjectName = SubjectName
		FROM Subject.tbSubject
		WHERE SubjectCode = @SubjectCode;

		IF @SubjectName IS NULL
			THROW 50000, 'Subject.proc_NextAddressCode: SubjectCode not found.', 1;

		EXEC App.proc_DefaultCodeGenerator
			@Description = @SubjectName,
			@CheckSql = @CheckSql,
			@UseWholeWords = 0,
			@Code = @GeneratedCode OUTPUT;

		SET @AddressCode = CAST(@GeneratedCode AS nvarchar(15));
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
GO
