CREATE TABLE [Cash].[tbTaxTagSource]
(
    TaxSourceCode      NVARCHAR(20)  NOT NULL,
    JurisdictionCode   NVARCHAR(10)  NOT NULL,   -- FK → App.tbJurisdiction
    SourceName         NVARCHAR(50)  NOT NULL,   -- e.g. 'MTD Company'
    SourceDescription  NVARCHAR(255) NULL,

    CONSTRAINT PK_Cash_tbTaxTagSource
        PRIMARY KEY CLUSTERED (TaxSourceCode),

    CONSTRAINT FK_Cash_tbTaxTagSource_Jurisdiction
        FOREIGN KEY (JurisdictionCode)
        REFERENCES App.tbJurisdiction(JurisdictionCode)
        ON DELETE CASCADE
);
GO
