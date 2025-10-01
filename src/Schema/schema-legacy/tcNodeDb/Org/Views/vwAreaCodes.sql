
CREATE   VIEW Org.vwAreaCodes
AS
SELECT        AreaCode
FROM            Org.tbOrg
GROUP BY AreaCode
HAVING        (AreaCode IS NOT NULL);
