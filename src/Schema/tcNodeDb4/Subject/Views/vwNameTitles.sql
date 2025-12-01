
CREATE   VIEW Subject.vwNameTitles
AS
SELECT        NameTitle
FROM            Subject.tbContact
GROUP BY NameTitle
HAVING        (NameTitle IS NOT NULL);
