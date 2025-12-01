CREATE   FUNCTION App.fnOffsetDays(@StartOn DATE, @EndOn DATE)
RETURNS SMALLINT
AS
BEGIN

	DECLARE 
		@OffsetDays SMALLINT = 0		  
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
			
	
	IF DATEDIFF(DAY, @StartOn, @EndOn) <= 0
		RETURN 0

	SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
	FROM         App.tbCalendar INNER JOIN
							Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
	WHERE UserId = (SELECT TOP (1) UserId FROM Usr.vwCredentials)
	
	WHILE @EndOn <> @StartOn
		BEGIN
		
		SET @CurrentDay = App.fnWeekDay(@EndOn)
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
						WHERE     (CalendarCode = @CalendarCode) AND (UnavailableOn = @EndOn))
				SET @OffsetDays += 1
			END
			
		SET @EndOn = DATEADD(d, -1, @EndOn)
		END

	
	RETURN @OffsetDays

END
