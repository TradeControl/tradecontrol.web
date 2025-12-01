CREATE   FUNCTION Cash.fnFlowCategoriesByType
	(
	@CashTypeCode smallint,
	@CategoryTypeCode smallint = 1
	)
RETURNS TABLE
AS
	RETURN (
		SELECT     Cash.tbCategory.DisplayOrder, Cash.tbCategory.Category, Cash.tbType.CashType, Cash.tbCategory.CategoryCode
		FROM         Cash.tbCategory INNER JOIN
							  Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode
		WHERE     ( Cash.tbCategory.CashTypeCode = @CashTypeCode) AND ( Cash.tbCategory.CategoryTypeCode = @CategoryTypeCode)
		)

