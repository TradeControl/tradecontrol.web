CREATE   VIEW Org.vwNamespace
AS

	WITH ancestors AS
	(
		SELECT CashAccountCode, HDPath.GetAncestor(1) Ancestor, HDPath, KeyName
		FROM Org.tbAccountKey
	), parent_child AS
	(
		SELECT nspace.CashAccountCode, nspace.HDPath parent, nspace.KeyName parentLoc, ancestors.HDPath child, ancestors.KeyName childLoc, ancestors.HDPath.GetLevel() KeyLevel
		FROM ancestors JOIN Org.tbAccountKey nspace ON ancestors.CashAccountCode = nspace.CashAccountCode AND ancestors.Ancestor = nspace.HDPath
	), namespaced AS
	(
		SELECT CashAccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, cast(KeyName AS nvarchar(1024)) KeyNamespace, HDPath.GetLevel() KeyLevel
		FROM Org.tbAccountKey
		WHERE HDPath = (SELECT DISTINCT hierarchyid::GetRoot() r FROM Org.tbAccountKey)

		UNION ALL

		SELECT parent_child.CashAccountCode, parent_child.parent ParentHDPath, parent_child.child ChildHDPath, cast(namespaced.KeyNamespace + '.' + parent_child.childLoc AS nvarchar(1024)) KeyNamespace, parent_child.KeyLevel
		FROM parent_child JOIN namespaced ON parent_child.CashAccountCode = namespaced.CashAccountCode AND parent_child.parent = namespaced.ChildHDPath
	)
	, hardened AS
	(
		SELECT namespaced.CashAccountCode, account.CoinTypeCode, namespaced.ChildHDPath HDPath, 
			REPLACE(namespaced.ParentHDPath.ToString(), '/', '''/') ParentHDPath, 
			REPLACE(namespaced.ChildHDPath.ToString(), '/', '''/') ChildHDPath, 
			KeyName, 
			REPLACE(UPPER(KeyNamespace), ' ', '_') KeyNamespace, 
			KeyLevel
		FROM namespaced
			JOIN Org.tbAccount account ON namespaced.CashAccountCode = account.CashAccountCode
			JOIN  Org.tbAccountKey ON namespaced.CashAccountCode = Org.tbAccountKey.CashAccountCode 
				AND namespaced.ChildHDPath = Org.tbAccountKey.HDPath
	)
	SELECT CashAccountCode,  -- HDPath, not supported VS
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ParentHDPath, LEN(ParentHDPath) - 1) AS nvarchar(50))) ParentHDPath, 
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ChildHDPath, LEN(ChildHDPath) - 1) AS nvarchar(50))) ChildHDPath, 
		KeyName,
		CAST(KeyNamespace AS nvarchar(1024)) KeyNamespace, KeyLevel, COALESCE(ReceiptIndex, 0) ReceiptIndex, COALESCE(ChangeIndex, 0) ChangeIndex 
	FROM hardened
		OUTER APPLY
		(
			SELECT COUNT(*) ReceiptIndex 
			FROM Cash.tbChange change
			WHERE change.CashAccountCode = hardened.CashAccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 0
		) receipts
		OUTER APPLY
		(
			SELECT COUNT(*) ChangeIndex 
			FROM Cash.tbChange change
			WHERE change.CashAccountCode = hardened.CashAccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 1
		) change;
