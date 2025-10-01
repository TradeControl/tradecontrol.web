
CREATE   VIEW Org.vwCompanyLogo
AS
SELECT        TOP (1) Org.tbOrg.Logo
FROM            Org.tbOrg INNER JOIN
                         App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode;
