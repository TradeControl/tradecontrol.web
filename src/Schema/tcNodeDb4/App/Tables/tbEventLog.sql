CREATE TABLE [App].[tbEventLog] (
    [LogCode]       NVARCHAR (20)  NOT NULL,
    [LoggedOn]      DATETIME       CONSTRAINT [DF_App_tbLog_LoggedOn] DEFAULT (getdate()) NOT NULL,
    [EventTypeCode] SMALLINT       CONSTRAINT [DF_App_tbLog_EventTypeCode] DEFAULT ((2)) NOT NULL,
    [EventMessage]  NVARCHAR (MAX) NULL,
    [InsertedBy]    NVARCHAR (50)  CONSTRAINT [DF_App_tbLog_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbEventLog_LogCode] PRIMARY KEY CLUSTERED ([LogCode] ASC),
    CONSTRAINT [FK_tbEventLog_EventType] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode])
);


GO
ALTER TABLE [App].[tbEventLog] NOCHECK CONSTRAINT [FK_tbEventLog_EventType];




GO
CREATE NONCLUSTERED INDEX [IX_App_tbEventLog_EventType]
    ON [App].[tbEventLog]([EventTypeCode] ASC, [LoggedOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_App_tbEventLog_LoggedOn]
    ON [App].[tbEventLog]([LoggedOn] DESC);

