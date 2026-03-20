CREATE FUNCTION App.fnDatasetMonths
(
	@LastClosedStartOn date
)
RETURNS TABLE
AS
RETURN
(
	WITH anchors AS
	(
		SELECT
			CAST(DATEADD
			(
				month,
				1,
				(
					SELECT MIN(CAST(StartOn AS date))
					FROM App.tbYearPeriod
					WHERE YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)
				)
			) AS date) AS StartOn,
			CAST(EOMONTH(@LastClosedStartOn) AS date) AS EndOn
	),
	m AS
	(
		SELECT a.StartOn AS MonthStartOn
		FROM anchors a

		UNION ALL

		SELECT DATEADD(month, 1, MonthStartOn)
		FROM m
		CROSS JOIN anchors a
		WHERE DATEADD(month, 1, MonthStartOn) <= a.EndOn
	)
	SELECT
		CAST(MonthStartOn AS date) AS MonthStartOn,
		CAST(DATEADD(day, -1, DATEADD(month, 1, MonthStartOn)) AS date) AS MonthEndOn,
		ROW_NUMBER() OVER (ORDER BY MonthStartOn) AS MonthIndex
	FROM m
);
