CREATE VIEW Subject.vwContacts
AS
	WITH ContactCount AS 
	(
		SELECT ContactName, COUNT(ProjectCode) AS Projects
        FROM Project.tbProject
        WHERE (ProjectStatusCode < 2)
        GROUP BY ContactName
        HAVING (ContactName IS NOT NULL)
	)
    SELECT Subject.tbContact.ContactName, Subject.tbSubject.SubjectCode, COALESCE(ContactCount.Projects, 0) Projects, Subject.tbContact.PhoneNumber, Subject.tbContact.HomeNumber, Subject.tbContact.MobileNumber,  
                              Subject.tbContact.EmailAddress, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Subject.tbContact.NameTitle, Subject.tbContact.NickName, Subject.tbContact.JobTitle, 
                              Subject.tbContact.Department, Subject.tbContact.Information, Subject.tbContact.InsertedBy, Subject.tbContact.InsertedOn
     FROM            Subject.tbSubject INNER JOIN
                              Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                              Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode INNER JOIN
                              Subject.tbContact ON Subject.tbSubject.SubjectCode = Subject.tbContact.SubjectCode LEFT OUTER JOIN
                              ContactCount ON Subject.tbContact.ContactName = ContactCount.ContactName
     WHERE        (Subject.tbSubject.SubjectStatusCode < 3);
