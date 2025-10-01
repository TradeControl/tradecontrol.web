
CREATE   VIEW Org.vwNameTitles
AS
SELECT        NameTitle
FROM            Org.tbContact
GROUP BY NameTitle
HAVING        (NameTitle IS NOT NULL);
