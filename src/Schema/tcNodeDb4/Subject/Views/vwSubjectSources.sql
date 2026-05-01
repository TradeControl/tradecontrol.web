
CREATE VIEW Subject.vwSubjectSources
AS
    SELECT SubjectSource
    FROM Subject.tbVirtual
    GROUP BY SubjectSource
    HAVING (SubjectSource IS NOT NULL);
