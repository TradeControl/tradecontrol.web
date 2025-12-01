CREATE   FUNCTION Subject.fnContactFileAs(@ContactName nvarchar(100))
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE @FileAs nvarchar(100)
		, @FirstNames nvarchar(100)
		, @LastName nvarchar(100)
		, @LastWordPos int;

	IF CHARINDEX(' ', @ContactName) = 0
		SET @FileAs = @ContactName
	ELSE
		BEGIN		
		SET @LastWordPos = CHARINDEX(' ', @ContactName) + 1
		WHILE CHARINDEX(' ', @ContactName, @LastWordPos) != 0
			SET @LastWordPos = CHARINDEX(' ', @ContactName, @LastWordPos) + 1
		
		SET @FirstNames = LEFT(@ContactName, @LastWordPos - 2)
		SET @LastName = RIGHT(@ContactName, LEN(@ContactName) - @LastWordPos + 1)
		SET @FileAs = @LastName + ', ' + @FirstNames
		END

	RETURN @FileAs
END
