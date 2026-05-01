CREATE PROCEDURE Subject.proc_DefaultAccountCode
(
    @AccountName nvarchar(100),
    @AccountCode nvarchar(10) OUTPUT
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE @CheckSql nvarchar(max) =
            N'SELECT @cnt = COUNT(AccountCode) FROM Subject.tbAccount WHERE AccountCode = @Code';

        EXEC App.proc_DefaultCodeGenerator
            @Description = @AccountName,
            @CheckSql = @CheckSql,
            @UseWholeWords = 0,
            @Code = @AccountCode OUTPUT;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
GO
