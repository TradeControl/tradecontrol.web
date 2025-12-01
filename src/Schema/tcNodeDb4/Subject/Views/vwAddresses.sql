
CREATE   VIEW Subject.vwAddresses
  AS
SELECT     TOP 100 PERCENT Subject.tbSubject.SubjectName, Subject.tbAddress.Address, Subject.tbSubject.SubjectTypeCode, Subject.tbSubject.SubjectStatusCode, 
                      Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Subject.vwMailContacts.ContactName, Subject.vwMailContacts.NickName, 
                      Subject.vwMailContacts.FormalName, Subject.vwMailContacts.JobTitle, Subject.vwMailContacts.Department
FROM         Subject.tbSubject INNER JOIN
                      Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                      Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                      Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode LEFT OUTER JOIN
                      Subject.vwMailContacts ON Subject.tbSubject.SubjectCode = Subject.vwMailContacts.SubjectCode
ORDER BY Subject.tbSubject.SubjectName

