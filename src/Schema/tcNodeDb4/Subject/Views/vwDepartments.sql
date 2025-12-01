
CREATE   VIEW Subject.vwDepartments
AS
SELECT        Department
FROM            Subject.tbContact
GROUP BY Department
HAVING        (Department IS NOT NULL);
