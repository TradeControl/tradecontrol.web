CREATE   FUNCTION App.fnBuckets
	(@CurrentDate datetime)
RETURNS  @tbBkn TABLE (Period smallint, BucketId nvarchar(10), StartDate datetime, EndDate datetime)
  AS
	BEGIN
	DECLARE @BucketTypeCode smallint
	DECLARE @UnitOfTimeCode smallint
	DECLARE @Period smallint	
	DECLARE @CurrentPeriod smallint
	DECLARE @Offset smallint
	
	DECLARE @StartDate datetime
	DECLARE @EndDate datetime
	DECLARE @BucketId nvarchar(10)
		
	SELECT     TOP 1 @BucketTypeCode = BucketTypeCode, @UnitOfTimeCode = BucketIntervalCode
	FROM         App.tbOptions
		
	SET @EndDate = 
		CASE @BucketTypeCode
			WHEN 0 THEN
				@CurrentDate
			WHEN 8 THEN
				DATEADD(d, Day(@CurrentDate) * -1 + 1, @CurrentDate)
			ELSE
				App.fnAdjustDateToBucket(@BucketTypeCode, @CurrentDate)
		END
			
	SET @EndDate = CAST(@EndDate AS date)
	SET @StartDate = DATEADD(yyyy, -100, @EndDate)
	SET @CurrentPeriod = 0
	
	DECLARE curBk cursor for			
		SELECT     Period, BucketId
		FROM         App.tbBucket
		ORDER BY Period

	OPEN curBk
	FETCH NEXT FROM curBk INTO @Period, @BucketId
	WHILE @@FETCH_STATUS = 0
		BEGIN
		IF @Period > 0
			BEGIN
			SET @StartDate = @EndDate
			SET @Offset = @Period - @CurrentPeriod
			SET @EndDate = CASE @UnitOfTimeCode
				WHEN 0 THEN		--day
					DATEADD(d, @Offset, @StartDate) 					
				WHEN 1 THEN		--week
					DATEADD(d, @Offset * 7, @StartDate)
				WHEN 2 THEN		--month
					DATEADD(m, @Offset, @StartDate)
				END
			END
		
		INSERT INTO @tbBkn(Period, BucketId, StartDate, EndDate)
		VALUES (@Period, @BucketId, @StartDate, @EndDate)
		
		SET @CurrentPeriod = @Period
		
		FETCH NEXT FROM curBk INTO @Period, @BucketId
		END		
			
	RETURN
	END
