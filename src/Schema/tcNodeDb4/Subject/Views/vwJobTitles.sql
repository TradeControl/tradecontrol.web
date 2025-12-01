
CREATE   VIEW Subject.vwJobTitles
AS
SELECT        JobTitle
FROM            Subject.tbContact
GROUP BY JobTitle
HAVING        (JobTitle IS NOT NULL);
