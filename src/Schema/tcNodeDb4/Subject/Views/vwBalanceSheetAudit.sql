CREATE   VIEW Subject.vwBalanceSheetAudit
AS
	SELECT        App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity, Cash.tbAssetType.AssetTypeCode, 
							 Cash.tbAssetType.AssetType, Subject.vwAssetBalances.StartOn, Subject.vwAssetBalances.Balance
	FROM            Subject.vwAssetBalances INNER JOIN
							 Cash.tbAssetType ON Subject.vwAssetBalances.AssetTypeCode = Cash.tbAssetType.AssetTypeCode INNER JOIN
							 Subject.tbSubject ON Subject.vwAssetBalances.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 App.tbYearPeriod ON Subject.vwAssetBalances.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode AND Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
							 Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Subject.vwAssetBalances.Balance <> 0) AND (Subject.vwAssetBalances.StartOn <= (SELECT TOP (1) StartOn FROM App.vwActivePeriod));
