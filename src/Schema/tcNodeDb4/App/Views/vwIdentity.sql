CREATE VIEW App.vwIdentity
AS
	SELECT TOP (1) Subject.tbSubject.SubjectName, Subject.tbAddress.Address, Subject.tbSubject.PhoneNumber, Subject.tbSubject.EmailAddress, Subject.tbSubject.WebSite, Subject.tbSubject.Logo, Usr.tbUser.UserName, Usr.tbUser.LogonName, Usr.tbUser.Avatar, 
							 Subject.tbSubject.CompanyNumber, Subject.tbSubject.VatNumber, App.tbUoc.UocName, App.tbUoc.UocSymbol
	FROM  Subject.tbSubject INNER JOIN
		App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode INNER JOIN
		App.tbUoc ON App.tbOptions.UnitOfCharge = App.tbUoc.UnitOfCharge LEFT OUTER JOIN
		Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode CROSS JOIN
		Usr.vwCredentials INNER JOIN
		Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId
