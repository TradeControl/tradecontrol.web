CREATE   FUNCTION Subject.fnKeyNamespace (@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE 
AS
	RETURN
	(
		WITH key_root AS
		(
			SELECT AccountCode, HDPath, HDLevel, KeyName
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND KeyName = @KeyName
		), candidates AS
		(
			SELECT AccountCode, HDPath.GetAncestor(1) ParentHDPath, HDPath ChildHDPath, KeyName
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND HDLevel > (SELECT HDLevel FROM key_root) 
		), namespace_set AS
		(
			SELECT AccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, KeyName FROM key_root

			UNION ALL

			SELECT candidates.AccountCode, candidates.ParentHDPath, candidates.ChildHDPath, candidates.KeyName
			FROM candidates
				JOIN namespace_set ON candidates.ParentHDPath = namespace_set.ChildHDPath
		)
		SELECT AccountCode, ChildHDPath HDPath, KeyName, Subject.fnAccountKeyNamespace(AccountCode, ChildHDPath) KeyNamespace
		FROM namespace_set
	)
