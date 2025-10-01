CREATE VIEW Cash.vwBalanceSheetOrgs
AS
	WITH asset_balances AS
	(
		SELECT AssetTypeCode, StartOn, SUM(Balance) Balance
		FROM Org.vwAssetBalances
		GROUP BY AssetTypeCode, StartOn
	)
	SELECT (SELECT CashAccountCode FROM Cash.vwCurrentAccount) AssetCode, asset_type.AssetType AssetName, 
		asset_type.AssetTypeCode,
		CASE asset_type.AssetTypeCode WHEN 0 THEN 1 ELSE 0 END CashModeCode,
		StartOn, Balance
	FROM asset_balances
		JOIN Cash.tbAssetType asset_type ON asset_balances.AssetTypeCode = asset_type.AssetTypeCode;
