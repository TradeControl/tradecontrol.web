
CREATE   VIEW Org.vwAccountSources
AS
SELECT        AccountSource
FROM            Org.tbOrg
GROUP BY AccountSource
HAVING        (AccountSource IS NOT NULL);
