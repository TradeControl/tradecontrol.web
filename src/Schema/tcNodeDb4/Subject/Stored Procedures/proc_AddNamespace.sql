CREATE PROCEDURE Subject.proc_AddNamespace
	(
	@RootSubjectCode nvarchar(50),
	@SubjectName nvarchar(100),
    @SubjectTypeCode smallint,
    @SubjectCode nvarchar(50) OUTPUT
	)
AS
SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY

	IF NULLIF(LTRIM(RTRIM(@SubjectName)), N'') IS NULL
		RAISERROR ('SubjectName is required.', 16, 1);

	IF NOT EXISTS
	(
		SELECT 1
		FROM Subject.tbSubject
		WHERE SubjectCode = @RootSubjectCode
	)
		RAISERROR ('Parent SubjectCode was not found.', 16, 1);

	IF EXISTS
	(
		SELECT 1
		FROM Subject.tbNamespace AS n
		INNER JOIN Subject.tbSubject AS s
			ON s.SubjectCode = n.ChildSubjectCode
		WHERE n.ParentSubjectCode = @RootSubjectCode
			AND s.SubjectName = @SubjectName
	)
	BEGIN
		SELECT TOP (1) @SubjectCode = s.SubjectCode
		FROM Subject.tbNamespace AS n
		INNER JOIN Subject.tbSubject AS s
			ON s.SubjectCode = n.ChildSubjectCode
		WHERE n.ParentSubjectCode = @RootSubjectCode
			AND s.SubjectName = @SubjectName;

		RETURN;
	END

	DECLARE
        @Ordinal int,
		@CheckSql nvarchar(max),
        @SubjectClassCode smallint;

    SELECT @SubjectClassCode = SubjectClassCode
    FROM Subject.tbType
    WHERE SubjectTypeCode = @SubjectTypeCode;

    IF @SubjectClassCode IS NULL
		RAISERROR ('SubjectTypeCode was not found.', 16, 1);

	SET @CheckSql = N'SELECT @cnt = COUNT(*) FROM Subject.tbSubject WHERE SubjectCode = @Code;';

	EXEC App.proc_DefaultCodeGenerator
		@Description = @SubjectName,
		@CheckSql = @CheckSql,
        @UseWholeWords = 1,
		@Code = @SubjectCode OUTPUT;

	IF LEN(ISNULL(@SubjectCode, N'')) = 0
		RAISERROR ('Unable to generate a SubjectCode.', 16, 1);

	BEGIN TRANSACTION;

	INSERT INTO Subject.tbSubject
	(
		SubjectCode,
		SubjectName,
		SubjectTypeCode,
		SubjectStatusCode,
		TransmitStatusCode,
		TaxCode,
		AddressCode,
		AreaCode,
		PhoneNumber,
		EmailAddress
	)
	SELECT
		@SubjectCode,
		@SubjectName,
		@SubjectTypeCode,
		SubjectStatusCode,
		TransmitStatusCode,
		TaxCode,
		AddressCode,
		AreaCode,
		PhoneNumber,
		EmailAddress
	FROM Subject.tbSubject
	WHERE SubjectCode = @RootSubjectCode;

    IF @SubjectClassCode = 0
    BEGIN
        INSERT INTO Subject.tbVirtual (SubjectCode)
        VALUES (@SubjectCode);
    END
    ELSE IF @SubjectClassCode = 1
    BEGIN
        INSERT INTO Subject.tbReal (SubjectCode)
        VALUES (@SubjectCode);
    END
    ELSE IF @SubjectClassCode = 2
    BEGIN
        INSERT INTO Subject.tbStructural (SubjectCode)
        VALUES (@SubjectCode);
    END
    ELSE
    BEGIN
		RAISERROR ('Unsupported SubjectClassCode.', 16, 1);
    END

	SELECT @Ordinal = ISNULL(MAX(Ordinal), 0) + 1
	FROM Subject.tbNamespace
	WHERE ParentSubjectCode = @RootSubjectCode;

	INSERT INTO Subject.tbNamespace
	(
		ParentSubjectCode,
		ChildSubjectCode,
		Ordinal
	)
	VALUES
	(
		@RootSubjectCode,
		@SubjectCode,
		@Ordinal
	);

	COMMIT TRANSACTION;

END TRY
BEGIN CATCH
	EXEC App.proc_ErrorLog;
END CATCH
GO
