CREATE   FUNCTION Org.fnKeyNamespace (@CashAccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE 
AS
	RETURN
	(
		WITH key_root AS
		(
			SELECT CashAccountCode, HDPath, HDLevel, KeyName
			FROM Org.tbAccountKey
			WHERE CashAccountCode = @CashAccountCode AND KeyName = @KeyName
		), candidates AS
		(
			SELECT CashAccountCode, HDPath.GetAncestor(1) ParentHDPath, HDPath ChildHDPath, KeyName
			FROM Org.tbAccountKey
			WHERE CashAccountCode = @CashAccountCode AND HDLevel > (SELECT HDLevel FROM key_root) 
		), namespace_set AS
		(
			SELECT CashAccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, KeyName FROM key_root

			UNION ALL

			SELECT candidates.CashAccountCode, candidates.ParentHDPath, candidates.ChildHDPath, candidates.KeyName
			FROM candidates
				JOIN namespace_set ON candidates.ParentHDPath = namespace_set.ChildHDPath
		)
		SELECT CashAccountCode, ChildHDPath HDPath, KeyName, Org.fnAccountKeyNamespace(CashAccountCode, ChildHDPath) KeyNamespace
		FROM namespace_set
	)
