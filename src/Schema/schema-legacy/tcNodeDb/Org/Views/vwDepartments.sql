
CREATE   VIEW Org.vwDepartments
AS
SELECT        Department
FROM            Org.tbContact
GROUP BY Department
HAVING        (Department IS NOT NULL);
