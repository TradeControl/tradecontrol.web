
CREATE   VIEW Task.vwCashMode
  AS
SELECT     Task.tbTask.TaskCode, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
                      THEN Org.tbType.CashModeCode ELSE Cash.tbCategory.CashModeCode END AS CashModeCode
FROM         Task.tbTask INNER JOIN
                      Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
                      Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                      Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode INNER JOIN
                      Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
