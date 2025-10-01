CREATE TABLE [Cash].[tbAssetType] (
    [AssetTypeCode] SMALLINT      NOT NULL,
    [AssetType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbAssetType] PRIMARY KEY CLUSTERED ([AssetTypeCode] ASC)
);

