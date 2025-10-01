CREATE TABLE [App].[tbBucketType] (
    [BucketTypeCode] SMALLINT      NOT NULL,
    [BucketType]     NVARCHAR (25) NOT NULL,
    CONSTRAINT [PK_App_tbBucketType] PRIMARY KEY CLUSTERED ([BucketTypeCode] ASC) WITH (FILLFACTOR = 90)
);

