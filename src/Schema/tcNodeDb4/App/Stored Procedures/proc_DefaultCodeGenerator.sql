CREATE PROCEDURE App.proc_DefaultCodeGenerator
(
    @Description nvarchar(100),
    @CheckSql nvarchar(max),          -- SQL must accept parameter @Code and return COUNT(*) into @cnt OUTPUT
    @UseWholeWords bit = 0,
    @Code nvarchar(50) OUTPUT
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE
            @ParsedName nvarchar(100),
            @FirstWord nvarchar(100),
            @SecondWord nvarchar(100),
            @ValidatedCode nvarchar(50),
            @BaseCode nvarchar(50),
            @Word nvarchar(100),
            @Work nvarchar(100),
            @ASCII smallint,
            @pos int,
            @ok bit,
            @Suffix smallint,
            @Rows int;

        SET @pos = 1;
        SET @ParsedName = N'';
        SET @Description = ISNULL(@Description, N'');

        WHILE @pos <= LEN(@Description)
        BEGIN
            SET @ASCII = ASCII(SUBSTRING(@Description, @pos, 1));
            SET @ok = CASE
                WHEN (@ASCII >= 48 AND @ASCII <= 57) THEN 1
                WHEN (@ASCII >= 65 AND @ASCII <= 90) THEN 1
                WHEN (@ASCII >= 97 AND @ASCII <= 122) THEN 1
                ELSE 0
            END;

            IF @ok = 1
                SET @ParsedName = @ParsedName + SUBSTRING(@Description, @pos, 1);
            ELSE
                SET @ParsedName = @ParsedName + N' ';

            SET @pos = @pos + 1;
        END

        SET @ParsedName = LTRIM(RTRIM(@ParsedName));

        WHILE CHARINDEX(N'  ', @ParsedName) > 0
            SET @ParsedName = REPLACE(@ParsedName, N'  ', N' ');

        IF LEN(@ParsedName) = 0
        BEGIN
            SET @Code = N'';
            RETURN;
        END

        IF @UseWholeWords = 0
        BEGIN
            IF CHARINDEX(N' ', @ParsedName) = 0
            BEGIN
                SET @FirstWord = @ParsedName;
                SET @SecondWord = N'';
            END
            ELSE
            BEGIN
                SET @FirstWord = LEFT(@ParsedName, CHARINDEX(N' ', @ParsedName) - 1);
                SET @SecondWord = RIGHT(@ParsedName, LEN(@ParsedName) - CHARINDEX(N' ', @ParsedName));

                IF CHARINDEX(N' ', @SecondWord) > 0
                    SET @SecondWord = LEFT(@SecondWord, CHARINDEX(N' ', @SecondWord) - 1);
            END

            IF EXISTS (SELECT 1 FROM App.tbCodeExclusion WHERE ExcludedTag = @SecondWord)
                SET @SecondWord = N'';

            IF LEN(@SecondWord) > 0
                SET @BaseCode = UPPER(LEFT(@FirstWord, 3)) + UPPER(LEFT(@SecondWord, 3));
            ELSE
                SET @BaseCode = UPPER(LEFT(@FirstWord, 6));

            SET @ValidatedCode = @BaseCode;
            SET @Suffix = 0;

            WHILE 1 = 1
            BEGIN
                DECLARE @cnt_legacy int;

                EXEC sys.sp_executesql
                    @CheckSql,
                    N'@Code nvarchar(50), @cnt int OUTPUT',
                    @Code = @ValidatedCode,
                    @cnt = @cnt_legacy OUTPUT;

                SET @Rows = ISNULL(@cnt_legacy, 0);

                IF @Rows = 0
                    BREAK;

                SET @Suffix = @Suffix + 1;
                SET @ValidatedCode = @BaseCode + CONVERT(nvarchar(10), @Suffix);

                IF LEN(@ValidatedCode) > 10
                    SET @ValidatedCode = LEFT(@ValidatedCode, 10);
            END
        END
        ELSE
        BEGIN
            SET @BaseCode = N'';
            SET @Work = @ParsedName;

            WHILE LEN(@Work) > 0
            BEGIN
                IF CHARINDEX(N' ', @Work) = 0
                BEGIN
                    SET @Word = @Work;
                    SET @Work = N'';
                END
                ELSE
                BEGIN
                    SET @Word = LEFT(@Work, CHARINDEX(N' ', @Work) - 1);
                    SET @Work = LTRIM(RIGHT(@Work, LEN(@Work) - CHARINDEX(N' ', @Work)));
                END

                IF LEN(@Word) > 0
                BEGIN
                    SET @BaseCode = @BaseCode
                        + UPPER(LEFT(@Word, 1))
                        + LOWER(SUBSTRING(@Word, 2, LEN(@Word)));
                END
            END

            SET @BaseCode = LEFT(@BaseCode, 50);
            SET @ValidatedCode = @BaseCode;
            SET @Suffix = 0;

            WHILE 1 = 1
            BEGIN
                DECLARE @cnt_whole int;

                EXEC sys.sp_executesql
                    @CheckSql,
                    N'@Code nvarchar(50), @cnt int OUTPUT',
                    @Code = @ValidatedCode,
                    @cnt = @cnt_whole OUTPUT;

                SET @Rows = ISNULL(@cnt_whole, 0);

                IF @Rows = 0
                    BREAK;

                SET @Suffix = @Suffix + 1;
                SET @ValidatedCode = LEFT(@BaseCode, 50 - LEN(CONVERT(nvarchar(10), @Suffix)))
                    + CONVERT(nvarchar(10), @Suffix);
            END
        END

        SET @Code = @ValidatedCode;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
GO
