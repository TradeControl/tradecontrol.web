CREATE FUNCTION [App].[fnToAlphaNumeric]
(
    @input nvarchar(MAX)
)
RETURNS nvarchar(MAX)
AS
BEGIN
    IF @input IS NULL RETURN NULL;

    DECLARE @out nvarchar(MAX) = N'';
    DECLARE @i int = 1, @len int = LEN(@input);

    WHILE @i <= @len
    BEGIN
        DECLARE @ch nchar(1) = SUBSTRING(@input, @i, 1);
        IF (@ch LIKE N'[A-Za-z0-9]' OR @ch = N'_' OR @ch = N'.')
            SET @out += @ch;
        ELSE IF @ch != ' '
            SET @out += N'_';
        SET @i += 1;
    END

    RETURN @out;
END