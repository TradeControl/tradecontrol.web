CREATE VIEW Org.vwContacts
AS
	WITH ContactCount AS 
	(
		SELECT ContactName, COUNT(TaskCode) AS Tasks
        FROM Task.tbTask
        WHERE (TaskStatusCode < 2)
        GROUP BY ContactName
        HAVING (ContactName IS NOT NULL)
	)
    SELECT Org.tbContact.ContactName, Org.tbOrg.AccountCode, COALESCE(ContactCount.Tasks, 0) Tasks, Org.tbContact.PhoneNumber, Org.tbContact.HomeNumber, Org.tbContact.MobileNumber,  
                              Org.tbContact.EmailAddress, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbStatus.OrganisationStatus, Org.tbContact.NameTitle, Org.tbContact.NickName, Org.tbContact.JobTitle, 
                              Org.tbContact.Department, Org.tbContact.Information, Org.tbContact.InsertedBy, Org.tbContact.InsertedOn
     FROM            Org.tbOrg INNER JOIN
                              Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                              Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode INNER JOIN
                              Org.tbContact ON Org.tbOrg.AccountCode = Org.tbContact.AccountCode LEFT OUTER JOIN
                              ContactCount ON Org.tbContact.ContactName = ContactCount.ContactName
     WHERE        (Org.tbOrg.OrganisationStatusCode < 3);
