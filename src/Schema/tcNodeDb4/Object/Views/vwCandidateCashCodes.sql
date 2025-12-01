CREATE VIEW Object.vwCandidateCashCodes
AS
	SELECT Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbCategory.CashPolarityCode, Cash.tbCategory.CashTypeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	WHERE        (Cash.tbCategory.CashTypeCode < 2)  AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCode.IsEnabled <> 0)
