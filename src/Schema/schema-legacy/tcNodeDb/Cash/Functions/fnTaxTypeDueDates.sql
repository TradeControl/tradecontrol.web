CREATE FUNCTION Cash.fnTaxTypeDueDates(@TaxTypeCode smallint)
RETURNS @tbDueDate TABLE (PayOn datetime, PayFrom datetime, PayTo datetime)
 AS
	BEGIN
	DECLARE @MonthNumber smallint
			, @MonthInterval smallint
			, @StartOn datetime
	
		SELECT 
			@MonthNumber = MonthNumber, 
			@MonthInterval = CASE RecurrenceCode
								WHEN 0 THEN 1
								WHEN 1 THEN 1
								WHEN 2 THEN 3
								WHEN 3 THEN 6
								WHEN 4 THEN 12
							END
		FROM Cash.tbTaxType
		WHERE TaxTypeCode = @TaxTypeCode			

		SELECT   @StartOn = MIN(StartOn)
		FROM         App.tbYearPeriod
		WHERE     (MonthNumber = @MonthNumber)

		INSERT INTO @tbDueDate (PayOn) VALUES (@StartOn)
	
		SET @MonthNumber = CASE 			
			WHEN (@MonthNumber + @MonthInterval) <= 12 THEN @MonthNumber + @MonthInterval
			WHEN (@MonthNumber + @MonthInterval) % 12 = 0 THEN @MonthNumber
			ELSE (@MonthNumber + @MonthInterval) % 12
			END
	
		WHILE EXISTS(SELECT     *
					 FROM         App.tbYearPeriod
					 WHERE     (StartOn > @StartOn) AND (MonthNumber = @MonthNumber))
		BEGIN
			SELECT @StartOn = MIN(StartOn)
			FROM         App.tbYearPeriod
			WHERE     (StartOn > @StartOn) AND (MonthNumber = @MonthNumber)
			ORDER BY MIN(StartOn)		
			INSERT INTO @tbDueDate (PayOn) VALUES (@StartOn)
		
			SET @MonthNumber = CASE 
						WHEN (@MonthNumber + @MonthInterval) <= 12 THEN @MonthNumber + @MonthInterval
						WHEN (@MonthNumber + @MonthInterval) % 12 = 0 THEN @MonthNumber
						ELSE (@MonthNumber + @MonthInterval) % 12 
						END;	
		END;

		WITH dd AS
		(
			SELECT PayOn, LAG(PayOn) OVER (ORDER BY PayOn) AS PayFrom
			FROM @tbDueDate 
		)
		UPDATE @tbDueDate
		SET PayTo = dd.PayOn, PayFrom = dd.PayFrom
		FROM @tbDueDate tbDueDate JOIN dd ON tbDueDate.PayOn = dd.PayOn;

		UPDATE @tbDueDate
		SET PayFrom = DATEADD(MONTH, @MonthInterval * -1, PayTo)
		WHERE PayTo = (SELECT MIN(PayTo) FROM @tbDueDate);

		UPDATE @tbDueDate
		SET PayOn = DATEADD(DAY, (SELECT OffsetDays FROM Cash.tbTaxType WHERE TaxTypeCode = @TaxTypeCode), PayOn)

	RETURN	
	END
