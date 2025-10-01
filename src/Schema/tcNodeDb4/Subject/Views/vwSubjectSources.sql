
CREATE   VIEW Subject.vwSubjectSources
AS
SELECT        SubjectSource
FROM            Subject.tbSubject
GROUP BY SubjectSource
HAVING        (SubjectSource IS NOT NULL);
