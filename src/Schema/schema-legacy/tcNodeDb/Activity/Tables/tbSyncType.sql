CREATE TABLE [Activity].[tbSyncType] (
    [SyncTypeCode] SMALLINT      NOT NULL,
    [SyncType]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Activity_tbSyncType] PRIMARY KEY CLUSTERED ([SyncTypeCode] ASC) WITH (FILLFACTOR = 90)
);

