
CREATE   VIEW App.vwHomeAccount
AS
	SELECT     Org.tbOrg.AccountCode, Org.tbOrg.AccountName
	FROM            App.tbOptions INNER JOIN
							 Org.tbOrg ON App.tbOptions.AccountCode = Org.tbOrg.AccountCode
