CREATE TABLE [App].[tbJurisdiction]
(
    JurisdictionCode   NVARCHAR(10)  NOT NULL,
    JurisdictionName   NVARCHAR(50)  NOT NULL,   -- e.g. 'United Kingdom'
    UocCode            NVARCHAR(5)   NOT NULL,   -- FK → App.tbUoc(UnitOfCharge)
    IsEnabled          BIT           NOT NULL CONSTRAINT DF_App_tbJurisdiction_IsEnabled DEFAULT 1,

    CONSTRAINT PK_App_tbJurisdiction
        PRIMARY KEY CLUSTERED (JurisdictionCode),

    CONSTRAINT FK_App_tbJurisdiction_Uoc
        FOREIGN KEY (UocCode) REFERENCES App.tbUoc(UnitOfCharge)
);
GO

CREATE UNIQUE INDEX [IX_tbJurisdiction_JurisdictionName]
ON [App].[tbJurisdiction] (JurisdictionName);
GO
