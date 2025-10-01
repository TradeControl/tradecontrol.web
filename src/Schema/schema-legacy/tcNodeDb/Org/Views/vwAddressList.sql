CREATE   VIEW Org.vwAddressList
AS
	SELECT        Org.tbOrg.AccountCode, Org.tbAddress.AddressCode, Org.tbOrg.AccountName, Org.tbStatus.OrganisationStatusCode, Org.tbStatus.OrganisationStatus, Org.tbType.OrganisationTypeCode, Org.tbType.OrganisationType, 
							 Org.tbAddress.Address, Org.tbAddress.InsertedBy, Org.tbAddress.InsertedOn, CAST(CASE WHEN Org.tbAddress.AddressCode = Org.tbOrg.AddressCode THEN 1 ELSE 0 END AS bit) AS IsAdminAddress
	FROM            Org.tbOrg INNER JOIN
							 Org.tbAddress ON Org.tbOrg.AccountCode = Org.tbAddress.AccountCode INNER JOIN
							 Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
