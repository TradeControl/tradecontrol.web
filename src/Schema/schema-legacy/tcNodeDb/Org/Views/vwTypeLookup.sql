
CREATE   VIEW Org.vwTypeLookup
AS
SELECT        Org.tbType.OrganisationTypeCode, Org.tbType.OrganisationType, Cash.tbMode.CashMode
FROM            Org.tbType INNER JOIN
                         Cash.tbMode ON Org.tbType.CashModeCode = Cash.tbMode.CashModeCode;
