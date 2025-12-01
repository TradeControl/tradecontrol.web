CREATE   VIEW Subject.vwNamespace
AS

	WITH ancestors AS
	(
		SELECT AccountCode, HDPath.GetAncestor(1) Ancestor, HDPath, KeyName
		FROM Subject.tbAccountKey
	), parent_child AS
	(
		SELECT nspace.AccountCode, nspace.HDPath parent, nspace.KeyName parentLoc, ancestors.HDPath child, ancestors.KeyName childLoc, ancestors.HDPath.GetLevel() KeyLevel
		FROM ancestors JOIN Subject.tbAccountKey nspace ON ancestors.AccountCode = nspace.AccountCode AND ancestors.Ancestor = nspace.HDPath
	), namespaced AS
	(
		SELECT AccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, cast(KeyName AS nvarchar(1024)) KeyNamespace, HDPath.GetLevel() KeyLevel
		FROM Subject.tbAccountKey
		WHERE HDPath = (SELECT DISTINCT hierarchyid::GetRoot() r FROM Subject.tbAccountKey)

		UNION ALL

		SELECT parent_child.AccountCode, parent_child.parent ParentHDPath, parent_child.child ChildHDPath, cast(namespaced.KeyNamespace + '.' + parent_child.childLoc AS nvarchar(1024)) KeyNamespace, parent_child.KeyLevel
		FROM parent_child JOIN namespaced ON parent_child.AccountCode = namespaced.AccountCode AND parent_child.parent = namespaced.ChildHDPath
	)
	, hardened AS
	(
		SELECT namespaced.AccountCode, account.CoinTypeCode, namespaced.ChildHDPath HDPath, 
			REPLACE(namespaced.ParentHDPath.ToString(), '/', '''/') ParentHDPath, 
			REPLACE(namespaced.ChildHDPath.ToString(), '/', '''/') ChildHDPath, 
			KeyName, 
			REPLACE(UPPER(KeyNamespace), ' ', '_') KeyNamespace, 
			KeyLevel
		FROM namespaced
			JOIN Subject.tbAccount account ON namespaced.AccountCode = account.AccountCode
			JOIN  Subject.tbAccountKey ON namespaced.AccountCode = Subject.tbAccountKey.AccountCode 
				AND namespaced.ChildHDPath = Subject.tbAccountKey.HDPath
	)
	SELECT AccountCode,  -- HDPath, not supported VS
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ParentHDPath, LEN(ParentHDPath) - 1) AS nvarchar(50))) ParentHDPath, 
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ChildHDPath, LEN(ChildHDPath) - 1) AS nvarchar(50))) ChildHDPath, 
		KeyName,
		CAST(KeyNamespace AS nvarchar(1024)) KeyNamespace, KeyLevel, COALESCE(ReceiptIndex, 0) ReceiptIndex, COALESCE(ChangeIndex, 0) ChangeIndex 
	FROM hardened
		OUTER APPLY
		(
			SELECT COUNT(*) ReceiptIndex 
			FROM Cash.tbChange change
			WHERE change.AccountCode = hardened.AccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 0
		) receipts
		OUTER APPLY
		(
			SELECT COUNT(*) ChangeIndex 
			FROM Cash.tbChange change
			WHERE change.AccountCode = hardened.AccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 1
		) change;
