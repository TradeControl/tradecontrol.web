CREATE   FUNCTION Cash.fnFlowCategoryCashCodes
	(
	@CategoryCode nvarchar(10)
	)
RETURNS TABLE
AS
	RETURN (
		SELECT     CashCode, CashDescription
		FROM         Cash.tbCode
		WHERE     (CategoryCode = @CategoryCode) AND (IsEnabled <> 0)			 
	)
