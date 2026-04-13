CREATE TABLE [Cash].[tbTaxTag]
(
    TaxSourceCode    NVARCHAR(20)   NOT NULL,   -- FK → Cash.tbTaxTagSource
    TagCode          NVARCHAR(64)   NOT NULL,   -- e.g. 'AC12'
    TagName          NVARCHAR(100)  NOT NULL,   -- e.g. 'Turnover'
    TagClassCode     TINYINT        NOT NULL CONSTRAINT DF_Cash_tbTaxTag_TagClassCode DEFAULT 1,
    TagDescription   NVARCHAR(MAX)  NULL,
    DisplayOrder     SMALLINT       NOT NULL CONSTRAINT DF_Cash_tbTaxTag_DisplayOrder DEFAULT 0,
    IsEnabled        BIT            NOT NULL CONSTRAINT DF_Cash_tbTaxTag_IsEnabled DEFAULT 1,

    CONSTRAINT PK_Cash_tbTaxTag
        PRIMARY KEY CLUSTERED (TaxSourceCode, TagCode),

    CONSTRAINT FK_Cash_tbTaxTag_Source
        FOREIGN KEY (TaxSourceCode)
        REFERENCES Cash.tbTaxTagSource(TaxSourceCode)
        ON DELETE CASCADE,

    CONSTRAINT FK_Cash_tbTaxTag_Class
        FOREIGN KEY (TagClassCode)
        REFERENCES Cash.tbTaxTagClass(TagClassCode)
);
GO
