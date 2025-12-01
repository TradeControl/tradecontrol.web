CREATE   FUNCTION Cash.fnFlowCategoryTotalCodes(@CategoryCode NVARCHAR(10))
RETURNS TABLE
AS
	RETURN
	(
		SELECT ChildCode AS CategoryCode FROM Cash.tbCategoryTotal WHERE ParentCode = @CategoryCode
	)
