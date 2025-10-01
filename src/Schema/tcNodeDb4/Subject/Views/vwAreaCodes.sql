
CREATE   VIEW Subject.vwAreaCodes
AS
SELECT        AreaCode
FROM            Subject.tbSubject
GROUP BY AreaCode
HAVING        (AreaCode IS NOT NULL);
