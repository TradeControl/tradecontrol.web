
CREATE   VIEW App.vwWarehouseOrg
AS
SELECT TOP (100) PERCENT Org.tbOrg.AccountCode, Org.tbDoc.DocumentName, Org.tbOrg.AccountName, Org.tbDoc.DocumentImage, Org.tbDoc.DocumentDescription, Org.tbDoc.InsertedBy, Org.tbDoc.InsertedOn, Org.tbDoc.UpdatedBy, 
                         Org.tbDoc.UpdatedOn, Org.tbDoc.RowVer
FROM            Org.tbOrg INNER JOIN
                         Org.tbDoc ON Org.tbOrg.AccountCode = Org.tbDoc.AccountCode
ORDER BY Org.tbDoc.AccountCode, Org.tbDoc.DocumentName;
