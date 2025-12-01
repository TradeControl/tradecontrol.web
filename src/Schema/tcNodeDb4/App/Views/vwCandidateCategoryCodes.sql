
CREATE   VIEW App.vwCandidateCategoryCodes
AS
	SELECT TOP 100 PERCENT CategoryCode, Category
	FROM            Cash.tbCategory
	WHERE        (CategoryTypeCode = 1)
	ORDER BY CategoryCode;
