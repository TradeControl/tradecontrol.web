CREATE PROCEDURE Usr.proc_DefaultUserId
(
	@UserName nvarchar(100),
	@UserId nvarchar(10) OUTPUT
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@ParsedName nvarchar(100),
			@FirstWord nvarchar(100),
			@SecondWord nvarchar(100),
			@ValidatedId nvarchar(10),
			@ASCII smallint,
			@pos int,
			@ok bit,
			@Suffix smallint,
			@Rows int;

		SET @pos = 1;
		SET @ParsedName = N'';

		WHILE @pos <= DATALENGTH(@UserName)
		BEGIN
			SET @ASCII = ASCII(SUBSTRING(@UserName, @pos, 1));
			SET @ok = CASE
				WHEN @ASCII = 32 THEN 1
				WHEN @ASCII = 45 THEN 1
				WHEN (@ASCII >= 48 and @ASCII <= 57) THEN 1
				WHEN (@ASCII >= 65 and @ASCII <= 90) THEN 1
				WHEN (@ASCII >= 97 and @ASCII <= 122) THEN 1
				ELSE 0
			END;

			IF @ok = 1
				SELECT @ParsedName = @ParsedName + CHAR(ASCII(SUBSTRING(@UserName, @pos, 1)));

			SET @pos = @pos + 1;
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

		IF LEN(@SecondWord) > 0
			SET @UserId = UPPER(LEFT(@FirstWord, 3)) + UPPER(LEFT(@SecondWord, 3));
		ELSE
			SET @UserId = UPPER(LEFT(@FirstWord, 6));

		-- enforce PK character rules
		IF App.fnParsePrimaryKey(@UserId) = 0
			SET @UserId = N'USER';

		SET @ValidatedId = @UserId;
		SELECT @Rows = COUNT(UserId) FROM Usr.tbUser WHERE UserId = @ValidatedId;
		SET @Suffix = 0;

		WHILE @Rows > 0 OR LEN(@ValidatedId) > 10
		BEGIN
			SET @Suffix = @Suffix + 1;
			SET @ValidatedId = LEFT(@UserId, 10 - LEN(LTRIM(STR(@Suffix)))) + LTRIM(STR(@Suffix));
			SELECT @Rows = COUNT(UserId) FROM Usr.tbUser WHERE UserId = @ValidatedId;
		END

		SET @UserId = @ValidatedId;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
