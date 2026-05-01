
CREATE VIEW Subject.vwNameTitles
AS
    SELECT NameTitle
    FROM Subject.tbReal
    GROUP BY NameTitle
    HAVING (NameTitle IS NOT NULL);
