
CREATE   VIEW Org.vwJobTitles
AS
SELECT        JobTitle
FROM            Org.tbContact
GROUP BY JobTitle
HAVING        (JobTitle IS NOT NULL);
