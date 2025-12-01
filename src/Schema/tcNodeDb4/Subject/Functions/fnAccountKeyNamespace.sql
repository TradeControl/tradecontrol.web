CREATE   FUNCTION Subject.fnAccountKeyNamespace
(
	@AccountCode nvarchar(10),
	@HDPath hierarchyid
) RETURNS NVARCHAR(512)
AS
BEGIN
	DECLARE @KeyNamespace nvarchar(512);

	WITH key_namespace AS
	(
		SELECT HDPath, HDPath.GetAncestor(1) Ancestor, CAST(KeyName as nvarchar(512)) KeyNamespace
		FROM Subject.tbAccountKey
		WHERE AccountCode = @AccountCode AND HDPath = @HDPath

		UNION ALL

		SELECT parent_key.HDPath, parent_key.HDPath.GetAncestor(1) Ancestor, CAST(CONCAT(parent_key.KeyName, '.', key_namespace.KeyNamespace) as nvarchar(512)) KeyNamespace
		FROM Subject.tbAccountKey parent_key
			JOIN key_namespace ON parent_key.HDPath = key_namespace.Ancestor
		WHERE AccountCode = @AccountCode
	)
	SELECT @KeyNamespace = REPLACE(UPPER(KeyNamespace), ' ', '_')
	FROM key_namespace

	RETURN @KeyNamespace
END
