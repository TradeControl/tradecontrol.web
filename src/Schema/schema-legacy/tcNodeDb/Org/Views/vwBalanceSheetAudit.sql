CREATE   VIEW Org.vwBalanceSheetAudit
AS
	SELECT        App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Cash.tbMode.CashMode, Cash.tbAssetType.AssetTypeCode, 
							 Cash.tbAssetType.AssetType, Org.vwAssetBalances.StartOn, Org.vwAssetBalances.Balance
	FROM            Org.vwAssetBalances INNER JOIN
							 Cash.tbAssetType ON Org.vwAssetBalances.AssetTypeCode = Cash.tbAssetType.AssetTypeCode INNER JOIN
							 Org.tbOrg ON Org.vwAssetBalances.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 App.tbYearPeriod ON Org.vwAssetBalances.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode AND Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
							 Cash.tbMode ON Org.tbType.CashModeCode = Cash.tbMode.CashModeCode AND Org.tbType.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Org.vwAssetBalances.Balance <> 0) AND (Org.vwAssetBalances.StartOn <= (SELECT TOP (1) StartOn FROM App.vwActivePeriod));
