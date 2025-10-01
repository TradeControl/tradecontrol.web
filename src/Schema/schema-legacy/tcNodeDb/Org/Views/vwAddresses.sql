
CREATE   VIEW Org.vwAddresses
  AS
SELECT     TOP 100 PERCENT Org.tbOrg.AccountName, Org.tbAddress.Address, Org.tbOrg.OrganisationTypeCode, Org.tbOrg.OrganisationStatusCode, 
                      Org.tbType.OrganisationType, Org.tbStatus.OrganisationStatus, Org.vwMailContacts.ContactName, Org.vwMailContacts.NickName, 
                      Org.vwMailContacts.FormalName, Org.vwMailContacts.JobTitle, Org.vwMailContacts.Department
FROM         Org.tbOrg INNER JOIN
                      Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode INNER JOIN
                      Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                      Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode LEFT OUTER JOIN
                      Org.vwMailContacts ON Org.tbOrg.AccountCode = Org.vwMailContacts.AccountCode
ORDER BY Org.tbOrg.AccountName

