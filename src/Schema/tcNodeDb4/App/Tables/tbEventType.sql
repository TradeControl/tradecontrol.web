CREATE TABLE [App].[tbEventType] (
    [EventTypeCode] SMALLINT      NOT NULL,
    [EventType]     NVARCHAR (15) NOT NULL,
    CONSTRAINT [PK_tbFeedLogEventCode] PRIMARY KEY CLUSTERED ([EventTypeCode] ASC)
);

