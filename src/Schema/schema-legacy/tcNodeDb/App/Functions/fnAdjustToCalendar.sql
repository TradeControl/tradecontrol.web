CREATE   FUNCTION App.fnAdjustToCalendar
	(
	@SourceDate datetime,
	@OffsetDays int
	)
RETURNS DATETIME
AS
BEGIN
	
	DECLARE 
		  @OutputDate datetime = @SourceDate
		, @CalendarCode nvarchar(10)
		, @WorkingDay bit
		, @CurrentDay smallint
		, @Monday smallint
		, @Tuesday smallint
		, @Wednesday smallint
		, @Thursday smallint
		, @Friday smallint
		, @Saturday smallint
		, @Sunday smallint
			

	SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
	FROM         App.tbCalendar INNER JOIN
							Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
	WHERE UserId = (SELECT TOP (1) UserId FROM Usr.vwCredentials)
	
	WHILE @OffsetDays > -1
		BEGIN
		SET @CurrentDay = App.fnWeekDay(@OutputDate)
		IF @CurrentDay = 1				
			SET @WorkingDay = CASE WHEN @Monday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 2
			SET @WorkingDay = CASE WHEN @Tuesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 3
			SET @WorkingDay = CASE WHEN @Wednesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 4
			SET @WorkingDay = CASE WHEN @Thursday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 5
			SET @WorkingDay = CASE WHEN @Friday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 6
			SET @WorkingDay = CASE WHEN @Saturday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 7
			SET @WorkingDay = CASE WHEN @Sunday != 0 THEN 1 ELSE 0 END
		
		IF @WorkingDay = 1
			BEGIN
			IF NOT EXISTS(SELECT     UnavailableOn
						FROM         App.tbCalendarHoliday
						WHERE     (CalendarCode = @CalendarCode) AND (UnavailableOn = @OutputDate))
				SET @OffsetDays -= 1
			END
			
		IF @OffsetDays > -1
			SET @OutputDate = DATEADD(d, -1, @OutputDate)
		END
	
	RETURN @OutputDate				
END
