CREATE   FUNCTION App.fnWeekDay
	(
	@Date datetime
	)
RETURNS smallint
    AS
	BEGIN
	DECLARE @CurrentDay smallint
	SET @CurrentDay = DATEPART(dw, @Date)
	RETURN 	CASE WHEN @CurrentDay > (7 - @@DATEFIRST + 1) THEN
				@CurrentDay - (7 - @@DATEFIRST + 1)
			ELSE
				@CurrentDay + (@@DATEFIRST - 1)
			END
	END
