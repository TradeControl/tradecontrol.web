CREATE VIEW Subject.vwEmailAddresses
AS
	SELECT SubjectCode, SubjectName ContactName, EmailAddress, CAST(1 as bit) IsAdmin
	FROM Subject.tbSubject
	WHERE (NOT (EmailAddress IS NULL))

	UNION

	SELECT ns.ParentSubjectCode as SubjectCode, c.ContactName, c.EmailAddress, CAST(0 as bit) IsAdmin
	FROM Subject.vwReal c
        JOIN Subject.tbNamespace ns
            ON c.SubjectCode = ns.ChildSubjectCode
	WHERE (NOT (c.EmailAddress IS NULL));
