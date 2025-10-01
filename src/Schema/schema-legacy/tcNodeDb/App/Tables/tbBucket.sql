CREATE TABLE [App].[tbBucket] (
    [Period]            SMALLINT      NOT NULL,
    [BucketId]          NVARCHAR (10) NOT NULL,
    [BucketDescription] NVARCHAR (50) NULL,
    [AllowForecasts]    BIT           NOT NULL,
    [RowVer]            ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbBucket] PRIMARY KEY CLUSTERED ([Period] ASC) WITH (FILLFACTOR = 90)
);

