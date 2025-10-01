CREATE VIEW App.vwIdentity
AS
	SELECT TOP (1) Org.tbOrg.AccountName, Org.tbAddress.Address, Org.tbOrg.PhoneNumber, Org.tbOrg.EmailAddress, Org.tbOrg.WebSite, Org.tbOrg.Logo, Usr.tbUser.UserName, Usr.tbUser.LogonName, Usr.tbUser.Avatar, 
							 Org.tbOrg.CompanyNumber, Org.tbOrg.VatNumber, App.tbUoc.UocName, App.tbUoc.UocSymbol
	FROM  Org.tbOrg INNER JOIN
		App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode INNER JOIN
		App.tbUoc ON App.tbOptions.UnitOfCharge = App.tbUoc.UnitOfCharge LEFT OUTER JOIN
		Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode CROSS JOIN
		Usr.vwCredentials INNER JOIN
		Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId
