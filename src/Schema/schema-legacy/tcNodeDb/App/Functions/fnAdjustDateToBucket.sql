CREATE   FUNCTION App.fnAdjustDateToBucket
	(
	@BucketDay smallint,
	@CurrentDate datetime
	)
RETURNS datetime
  AS
	BEGIN
	DECLARE @CurrentDay smallint
	DECLARE @Offset smallint
	DECLARE @AdjustedDay smallint
	
	SET @CurrentDay = DATEPART(dw, @CurrentDate)
	
	SET @AdjustedDay = CASE WHEN @CurrentDay > (7 - @@DATEFIRST + 1) THEN
				@CurrentDay - (7 - @@DATEFIRST + 1)
			ELSE
				@CurrentDay + (@@DATEFIRST - 1)
			END

	SET @Offset = CASE WHEN @BucketDay <= @AdjustedDay THEN
				@BucketDay - @AdjustedDay
			ELSE
				(7 - (@BucketDay - @AdjustedDay)) * -1
			END
	
		
	RETURN DATEADD(dd, @Offset, @CurrentDate)
	END
