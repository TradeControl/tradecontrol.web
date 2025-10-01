
CREATE   VIEW App.vwCandidateHomeAccounts
AS
SELECT        Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Cash.tbMode.CashMode
FROM            Org.tbOrg INNER JOIN
                         Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                         Cash.tbMode ON Org.tbType.CashModeCode = Cash.tbMode.CashModeCode
WHERE        (Org.tbOrg.OrganisationStatusCode < 3);
