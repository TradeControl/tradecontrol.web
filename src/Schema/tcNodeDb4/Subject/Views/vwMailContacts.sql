
CREATE VIEW Subject.vwMailContacts
AS
    SELECT SubjectCode, ContactName, NickName, NameTitle + N' ' + ContactName AS FormalName, JobTitle, Department
    FROM Subject.vwReal
    WHERE (OnMailingList <> 0);

