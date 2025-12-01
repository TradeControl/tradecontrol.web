
CREATE   VIEW Subject.vwTypeLookup
AS
SELECT        Subject.tbType.SubjectTypeCode, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity
FROM            Subject.tbType INNER JOIN
                         Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode;
