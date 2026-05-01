
CREATE VIEW Subject.vwJobTitles
AS
    SELECT JobTitle
    FROM Subject.tbReal
    GROUP BY JobTitle
    HAVING (JobTitle IS NOT NULL);
