CREATE VIEW Org.vwAssetBalances
AS
	WITH financial_periods AS
	(
		SELECT pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), org_periods AS
	(
		SELECT AccountCode, StartOn
		FROM Org.tbOrg orgs
			CROSS JOIN financial_periods	
	)
	, org_statements AS
	(
		SELECT StartOn, 
			AccountCode, os.RowNumber, TransactedOn, Balance,
			MAX(RowNumber) OVER (PARTITION BY AccountCode, StartOn ORDER BY StartOn) LastRowNumber 
		FROM Org.vwAssetStatement os
		WHERE TransactedOn >= (SELECT StartOn FROM Cash.vwBalanceStartOn)
	)
	, org_balances AS
	(
		SELECT AccountCode, StartOn, Balance
		FROM org_statements
		WHERE RowNumber = LastRowNumber
	)
	, org_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY org_periods.AccountCode, org_periods.StartOn) EntryNumber,
			org_periods.AccountCode, org_periods.StartOn, 
			COALESCE(Balance, 0) Balance,
			CASE WHEN org_balances.StartOn IS NULL THEN 0 ELSE 1 END IsEntry
		FROM org_periods
			LEFT OUTER JOIN org_balances 
				ON org_periods.AccountCode = org_balances.AccountCode AND org_periods.StartOn = org_balances.StartOn
	), org_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY AccountCode, IsEntry ORDER BY EntryNumber) RNK
		FROM org_ordered
	), org_grouped AS
	(
		SELECT EntryNumber, AccountCode, StartOn, IsEntry, Balance,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM org_ranked
	)
	, org_projected AS
	(
		SELECT EntryNumber, AccountCode, StartOn, IsEntry,
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END
			AS Balance
		FROM org_grouped	
	), org_entries AS
	(
		SELECT AccountCode, EntryNumber, StartOn, Balance * -1 AS Balance,
			CASE 
				WHEN Balance < 0 THEN 0 
				ELSE 1
			END AS AssetTypeCode, 
			CASE WHEN Balance <> 0 THEN 1 ELSE IsEntry END AS IsEntry
		FROM org_projected
	)
	SELECT AccountCode, StartOn, Balance, 
		CASE 
			WHEN Balance <> 0 THEN AssetTypeCode 
			ELSE
				COALESCE(LAG(AssetTypeCode) OVER (PARTITION BY AccountCode ORDER BY EntryNumber), 0)
		END AssetTypeCode
	FROM org_entries WHERE IsEntry = 1;
