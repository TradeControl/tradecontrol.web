CREATE TABLE [App].[tbBucketInterval] (
    [BucketIntervalCode] SMALLINT      NOT NULL,
    [BucketInterval]     NVARCHAR (15) NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbBucketInterval] PRIMARY KEY CLUSTERED ([BucketIntervalCode] ASC) WITH (FILLFACTOR = 90)
);

