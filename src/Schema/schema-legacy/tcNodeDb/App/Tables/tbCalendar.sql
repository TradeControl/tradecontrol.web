CREATE TABLE [App].[tbCalendar] (
    [CalendarCode] NVARCHAR (10) NOT NULL,
    [Monday]       BIT           CONSTRAINT [DF_App_tbCalendar_Monday] DEFAULT ((1)) NOT NULL,
    [Tuesday]      BIT           CONSTRAINT [DF_App_tbCalendar_Tuesday] DEFAULT ((1)) NOT NULL,
    [Wednesday]    BIT           CONSTRAINT [DF_App_tbCalendar_Wednesday] DEFAULT ((1)) NOT NULL,
    [Thursday]     BIT           CONSTRAINT [DF_App_tbCalendar_Thursday] DEFAULT ((1)) NOT NULL,
    [Friday]       BIT           CONSTRAINT [DF_App_tbCalendar_Friday] DEFAULT ((1)) NOT NULL,
    [Saturday]     BIT           CONSTRAINT [DF_App_tbCalendar_Saturday] DEFAULT ((0)) NOT NULL,
    [Sunday]       BIT           CONSTRAINT [DF_App_tbCalendar_Sunday] DEFAULT ((0)) NOT NULL,
    [RowVer]       ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbCalendar] PRIMARY KEY CLUSTERED ([CalendarCode] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE   TRIGGER App.App_tbCalendar_TriggerUpdate 
   ON  App.tbCalendar
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CalendarCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
