CREATE VIEW Subject.vwContacts
AS
    WITH ContactCount AS
    (
	    SELECT
	     ContactName,
	     COUNT(ProjectCode) AS Projects
	    FROM
	     Project.tbProject
	    WHERE
	     (ProjectStatusCode < 2)
	    GROUP BY ContactName
	    HAVING ContactName IS NOT NULL 
    )
    SELECT
	    c.ContactName,
	    Subject.tbSubject.SubjectCode,
	    COALESCE(ContactCount.Projects,0) Projects,
	    c.PhoneNumber,
	    c.HomeNumber,
	    c.MobileNumber,
	    c.EmailAddress,
	    Subject.tbSubject.SubjectName,
	    Subject.tbType.SubjectType,
	    Subject.tbStatus.SubjectStatus,
	    c.NameTitle,
	    c.NickName,
	    c.JobTitle,
	    c.Department,
	    c.Information,
	    c.InsertedBy,
	    c.InsertedOn
    FROM
      Subject.tbSubject
    INNER JOIN
      Subject.tbType
    ON
      Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
    INNER JOIN
      Subject.tbStatus
    ON
      Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode
    INNER JOIN
      Subject.vwReal c
    ON
      Subject.tbSubject.SubjectCode = c.SubjectCode
    LEFT OUTER JOIN
      ContactCount
    ON
      c.ContactName = ContactCount.ContactName
    WHERE
      Subject.tbSubject.SubjectStatusCode < 3;
