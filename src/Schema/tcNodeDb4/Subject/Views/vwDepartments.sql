
CREATE VIEW Subject.vwDepartments
AS
    SELECT Department
    FROM Subject.tbReal
    GROUP BY Department
    HAVING (Department IS NOT NULL);
