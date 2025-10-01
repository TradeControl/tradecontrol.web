CREATE VIEW App.vwVersion
AS
	SELECT CONCAT(ROUND(SQLDataVersion, 3), '.', SQLRelease) AS VersionString, ROUND(SQLDataVersion, 3) SQLDataVersion, SQLRelease
	FROM App.tbInstall
	WHERE InstallId = (SELECT MAX(InstallId) FROM App.tbInstall)
