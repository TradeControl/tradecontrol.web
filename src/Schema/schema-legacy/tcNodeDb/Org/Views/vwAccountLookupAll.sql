CREATE   VIEW Org.vwAccountLookupAll
AS
	SELECT Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbOrg.OrganisationTypeCode, Org.tbType.OrganisationType, Cash.tbMode.CashMode, Cash.tbMode.CashModeCode, Org.tbOrg.OrganisationStatusCode, Org.tbStatus.OrganisationStatus
	FROM Org.tbOrg 
		JOIN Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
		JOIN Cash.tbMode ON Org.tbType.CashModeCode = Cash.tbMode.CashModeCode 
		JOIN Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode;

