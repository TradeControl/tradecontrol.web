
CREATE   PROCEDURE App.proc_AdjustToCalendar
	(
	@SourceDate datetime,
	@OffsetDays int,
	@OutputDate datetime output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@CalendarCode nvarchar(10)
			, @WorkingDay bit
			, @UserId nvarchar(10)
	
		DECLARE
			 @CurrentDay smallint
			, @Monday smallint
			, @Tuesday smallint
			, @Wednesday smallint
			, @Thursday smallint
			, @Friday smallint
			, @Saturday smallint
			, @Sunday smallint
		
		SELECT @UserId = UserId
		FROM         Usr.vwCredentials	

		SET @OutputDate = @SourceDate

		SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
		FROM         App.tbCalendar INNER JOIN
							  Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
		WHERE UserId = @UserId
	
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
					
		

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

