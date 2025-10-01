
CREATE   VIEW Org.vwListActive
AS
	SELECT        TOP (100) PERCENT Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbType.CashModeCode
	FROM            Org.tbOrg INNER JOIN
							 Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
	WHERE        (Task.tbTask.TaskStatusCode = 1 OR
							 Task.tbTask.TaskStatusCode = 2) AND (Task.tbTask.CashCode IS NOT NULL)
	GROUP BY Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbType.CashModeCode
	ORDER BY Org.tbOrg.AccountName;
