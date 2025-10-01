CREATE   VIEW Org.vwEmailAddresses
AS
	SELECT AccountCode, AccountName ContactName, EmailAddress, CAST(1 as bit) IsAdmin
	FROM Org.tbOrg
	WHERE (NOT (EmailAddress IS NULL))
	UNION
	SELECT AccountCode, ContactName, EmailAddress, CAST(0 as bit) IsAdmin
	FROM            Org.tbContact
	WHERE        (NOT (EmailAddress IS NULL))
