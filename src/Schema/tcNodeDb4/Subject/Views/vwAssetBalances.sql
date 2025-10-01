CREATE VIEW Subject.vwAssetBalances
AS
	WITH financial_periods AS
	(
		SELECT pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), Subject_periods AS
	(
		SELECT SubjectCode, StartOn
		FROM Subject.tbSubject Subjects
			CROSS JOIN financial_periods	
	)
	, Subject_statements AS
	(
		SELECT StartOn, 
			SubjectCode, os.RowNumber, TransactedOn, Balance,
			MAX(RowNumber) OVER (PARTITION BY SubjectCode, StartOn ORDER BY StartOn) LastRowNumber 
		FROM Subject.vwAssetStatement os
		WHERE TransactedOn >= (SELECT StartOn FROM Cash.vwBalanceStartOn)
	)
	, Subject_balances AS
	(
		SELECT SubjectCode, StartOn, Balance
		FROM Subject_statements
		WHERE RowNumber = LastRowNumber
	)
	, Subject_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY Subject_periods.SubjectCode, Subject_periods.StartOn) EntryNumber,
			Subject_periods.SubjectCode, Subject_periods.StartOn, 
			COALESCE(Balance, 0) Balance,
			CASE WHEN Subject_balances.StartOn IS NULL THEN 0 ELSE 1 END IsEntry
		FROM Subject_periods
			LEFT OUTER JOIN Subject_balances 
				ON Subject_periods.SubjectCode = Subject_balances.SubjectCode AND Subject_periods.StartOn = Subject_balances.StartOn
	), Subject_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY SubjectCode, IsEntry ORDER BY EntryNumber) RNK
		FROM Subject_ordered
	), Subject_grouped AS
	(
		SELECT EntryNumber, SubjectCode, StartOn, IsEntry, Balance,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY SubjectCode ORDER BY EntryNumber) RNK
		FROM Subject_ranked
	)
	, Subject_projected AS
	(
		SELECT EntryNumber, SubjectCode, StartOn, IsEntry,
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY SubjectCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY SubjectCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END
			AS Balance
		FROM Subject_grouped	
	), Subject_entries AS
	(
		SELECT SubjectCode, EntryNumber, StartOn, Balance * -1 AS Balance,
			CASE 
				WHEN Balance < 0 THEN 0 
				ELSE 1
			END AS AssetTypeCode, 
			CASE WHEN Balance <> 0 THEN 1 ELSE IsEntry END AS IsEntry
		FROM Subject_projected
	)
	SELECT SubjectCode, StartOn, Balance, 
		CASE 
			WHEN Balance <> 0 THEN AssetTypeCode 
			ELSE
				COALESCE(LAG(AssetTypeCode) OVER (PARTITION BY SubjectCode ORDER BY EntryNumber), 0)
		END AssetTypeCode
	FROM Subject_entries WHERE IsEntry = 1;
