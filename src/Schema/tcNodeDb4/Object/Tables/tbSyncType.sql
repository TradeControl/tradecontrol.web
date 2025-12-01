CREATE TABLE [Object].[tbSyncType] (
    [SyncTypeCode] SMALLINT      NOT NULL,
    [SyncType]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Object_tbSyncType] PRIMARY KEY CLUSTERED ([SyncTypeCode] ASC) WITH (FILLFACTOR = 90)
);

