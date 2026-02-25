CREATE PROCEDURE App.proc_DefaultTaxCode
(
    @TaxDescription nvarchar(100),
    @TaxCode nvarchar(10) OUTPUT
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE @CheckSql nvarchar(max) =
            N'SELECT @cnt = COUNT(*) FROM App.tbTaxCode WHERE TaxCode = @Code';

        EXEC App.proc_DefaultCodeGenerator
            @Description = @TaxDescription,
            @CheckSql = @CheckSql,
            @Code = @TaxCode OUTPUT;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
