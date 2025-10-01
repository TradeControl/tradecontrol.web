CREATE   VIEW Org.vwCompanyHeader
AS
SELECT        TOP (1) Org.tbOrg.AccountName AS CompanyName, Org.tbAddress.Address AS CompanyAddress, Org.tbOrg.PhoneNumber AS CompanyPhoneNumber, 
                         Org.tbOrg.EmailAddress AS CompanyEmailAddress, Org.tbOrg.WebSite AS CompanyWebsite, Org.tbOrg.CompanyNumber, Org.tbOrg.VatNumber
FROM            Org.tbOrg INNER JOIN
                         App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode LEFT OUTER JOIN
                         Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode;
