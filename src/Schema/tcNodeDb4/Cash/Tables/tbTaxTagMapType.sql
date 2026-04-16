CREATE TABLE [Cash].[tbTaxTagMapType]
(
    [MapTypeCode] TINYINT       NOT NULL,
    [MapType]     NVARCHAR(20)  NOT NULL,

    CONSTRAINT [PK_Cash_tbTaxTagMapType]
        PRIMARY KEY CLUSTERED ([MapTypeCode] ASC)
);
GO
