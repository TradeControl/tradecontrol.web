CREATE PROCEDURE App.proc_DefaultCodeGenerator
(
    @Description nvarchar(100),
    @CheckSql nvarchar(max),          -- SQL must accept parameter @Code and return COUNT(*) into @cnt OUTPUT
    @Code nvarchar(10) OUTPUT
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE
            @ParsedName nvarchar(100),
            @FirstWord nvarchar(100),
            @SecondWord nvarchar(100),
            @ValidatedCode nvarchar(10),
            @ASCII smallint,
            @pos int,
            @ok bit,
            @Suffix smallint,
            @Rows int;

        SET @pos = 1;
        SET @ParsedName = N'';
        SET @Description = ISNULL(@Description, N'');

        WHILE @pos <= DATALENGTH(@Description)
        BEGIN
            SET @ASCII = ASCII(SUBSTRING(@Description, @pos, 1));
            SET @ok = CASE
                WHEN @ASCII = 32 THEN 1   -- space
                WHEN @ASCII = 45 THEN 1   -- hyphen
                WHEN (@ASCII >= 48 and @ASCII <= 57) THEN 1
                WHEN (@ASCII >= 65 and @ASCII <= 90) THEN 1
                WHEN (@ASCII >= 97 and @ASCII <= 122) THEN 1
                ELSE 0
            END;

            IF @ok = 1
                SELECT @ParsedName = @ParsedName + CHAR(ASCII(SUBSTRING(@Description, @pos, 1)));

            SET @pos = @pos + 1;
        END

        SET @ParsedName = LTRIM(RTRIM(@ParsedName));

        IF LEN(@ParsedName) = 0
        BEGIN
            SET @Code = N'';
            RETURN;
        END

        IF CHARINDEX(' ', @ParsedName) = 0
        BEGIN
            SET @FirstWord = @ParsedName;
            SET @SecondWord = N'';
        END
        ELSE
        BEGIN
            SET @FirstWord = LEFT(@ParsedName, CHARINDEX(' ', @ParsedName) - 1);
            SET @SecondWord = RIGHT(@ParsedName, LEN(@ParsedName) - CHARINDEX(' ', @ParsedName));

            IF CHARINDEX(' ', @SecondWord) > 0
                SET @SecondWord = LEFT(@SecondWord, CHARINDEX(' ', @SecondWord) - 1);
        END

        IF EXISTS(SELECT ExcludedTag FROM App.tbCodeExclusion WHERE ExcludedTag = @SecondWord)
            SET @SecondWord = N'';

        IF LEN(@SecondWord) > 0
            SET @Code = UPPER(LEFT(@FirstWord, 3)) + UPPER(LEFT(@SecondWord, 3));
        ELSE
            SET @Code = UPPER(LEFT(@FirstWord, 6));

        SET @ValidatedCode = @Code;
        SET @Suffix = 0;

        WHILE 1 = 1
        BEGIN
            DECLARE @cnt int;

            EXEC sys.sp_executesql
                @CheckSql,
                N'@Code nvarchar(10), @cnt int OUTPUT',
                @Code = @ValidatedCode,
                @cnt = @cnt OUTPUT;

            SET @Rows = ISNULL(@cnt, 0);

            IF @Rows = 0
                BREAK;

            SET @Suffix = @Suffix + 1;
            SET @ValidatedCode = @Code + LTRIM(STR(@Suffix));

            IF LEN(@ValidatedCode) > 10
                SET @ValidatedCode = LEFT(@ValidatedCode, 10);
        END

        SET @Code = @ValidatedCode;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
