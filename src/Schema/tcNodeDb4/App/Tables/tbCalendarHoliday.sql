CREATE TABLE [App].[tbCalendarHoliday] (
    [CalendarCode]  NVARCHAR (10) NOT NULL,
    [UnavailableOn] DATETIME      NOT NULL,
    [RowVer]        ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbCalendarHoliday] PRIMARY KEY CLUSTERED ([CalendarCode] ASC, [UnavailableOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbCalendarHoliday_tbCalendar] FOREIGN KEY ([CalendarCode]) REFERENCES [App].[tbCalendar] ([CalendarCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_App_tbCalendarHoliday_CalendarCode]
    ON [App].[tbCalendarHoliday]([CalendarCode] ASC) WITH (FILLFACTOR = 90);

