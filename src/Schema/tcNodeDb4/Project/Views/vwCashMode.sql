
CREATE   VIEW Project.vwCashPolarity
  AS
SELECT     Project.tbProject.ProjectCode, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
                      THEN Subject.tbType.CashPolarityCode ELSE Cash.tbCategory.CashPolarityCode END AS CashPolarityCode
FROM         Project.tbProject INNER JOIN
                      Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
                      Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                      Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                      Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
