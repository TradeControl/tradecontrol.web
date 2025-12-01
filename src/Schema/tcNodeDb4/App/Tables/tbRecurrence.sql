CREATE TABLE [App].[tbRecurrence] (
    [RecurrenceCode] SMALLINT      NOT NULL,
    [Recurrence]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_App_tbRecurrence] PRIMARY KEY CLUSTERED ([RecurrenceCode] ASC) WITH (FILLFACTOR = 90)
);

