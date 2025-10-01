
CREATE FUNCTION App.fnVersion()
RETURNS NVARCHAR(10)
AS
BEGIN
	DECLARE @Version NVARCHAR(10) = '0.0.0'
	SELECT @Version = VersionString
	FROM App.vwVersion
	RETURN @Version
END
