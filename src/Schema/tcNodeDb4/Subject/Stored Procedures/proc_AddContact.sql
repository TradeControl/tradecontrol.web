CREATE PROCEDURE Subject.proc_AddContact 
	(
	@SubjectCode nvarchar(50),
	@ContactName nvarchar(100)	 
	)
AS
SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY
    DECLARE
        @ContactSubjectCode nvarchar(50)
        , @ContactSubjectTypeCode smallint;

	SELECT TOP (1)
		@ContactSubjectTypeCode = SubjectTypeCode
	FROM Subject.tbType
	WHERE SubjectClassCode = 1
	ORDER BY SubjectTypeCode;

	IF @ContactSubjectTypeCode IS NULL
		RAISERROR ('No real SubjectType is available for contact creation.', 16, 1);

    EXECUTE Subject.proc_AddNamespace
        @RootSubjectCode = @SubjectCode
        , @SubjectName = @ContactName
        , @SubjectTypeCode = @ContactSubjectTypeCode
        , @SubjectCode = @ContactSubjectCode OUTPUT;

END TRY
BEGIN CATCH
	EXEC App.proc_ErrorLog;
END CATCH

GO
    
