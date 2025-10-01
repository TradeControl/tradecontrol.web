CREATE   FUNCTION Cash.fnFlowCategory(@CashTypeCode smallint)
RETURNS @tbCategory TABLE (CategoryCode nvarchar(10), Category nvarchar(50), CashModeCode smallint, DisplayOrder smallint)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Cash.vwCategoryCapital capital 
						JOIN Cash.tbCategory category ON capital.CategoryCode = category.CategoryCode 
						WHERE (category.CategoryTypeCode = 0) AND (category.CashTypeCode = @CashTypeCode) AND (category.IsEnabled <> 0))
	BEGIN
		INSERT INTO @tbCategory (CategoryCode, Category, CashModeCode, DisplayOrder)
		SELECT CategoryCode, Category, CashModeCode, DisplayOrder
		FROM Cash.tbCategory
		WHERE (CategoryTypeCode = 0) AND (CashTypeCode = @CashTypeCode) AND (IsEnabled <> 0)		
	END
	ELSE
	BEGIN
		INSERT INTO @tbCategory (CategoryCode, Category, CashModeCode, DisplayOrder)
		SELECT CategoryCode, Category, CashModeCode, DisplayOrder
		FROM Cash.vwCategoryCapital
	END

	RETURN
END
