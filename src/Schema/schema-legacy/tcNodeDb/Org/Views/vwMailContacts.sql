
CREATE   VIEW Org.vwMailContacts
  AS
SELECT     AccountCode, ContactName, NickName, NameTitle + N' ' + ContactName AS FormalName, JobTitle, Department
FROM         Org.tbContact
WHERE     (OnMailingList <> 0)

