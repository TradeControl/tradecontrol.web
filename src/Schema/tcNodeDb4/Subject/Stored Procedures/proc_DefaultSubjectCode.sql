CREATE PROCEDURE Subject.proc_DefaultSubjectCode 
(
    @SubjectName nvarchar(100),
    @SubjectCode nvarchar(10) OUTPUT 
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE @CheckSql nvarchar(max) =
            N'SELECT @cnt = COUNT(SubjectCode) FROM Subject.tbSubject WHERE SubjectCode = @Code';

        EXEC App.proc_DefaultCodeGenerator
            @Description = @SubjectName,
            @CheckSql = @CheckSql,
            @Code = @SubjectCode OUTPUT;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
