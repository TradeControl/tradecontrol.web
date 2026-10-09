SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF SCHEMA_ID(N'TaxHub') IS NULL EXEC(N'CREATE SCHEMA TaxHub AUTHORIZATION dbo;');
GO

IF OBJECT_ID(N'TaxHub.CompaniesHousePreparation', N'U') IS NULL
BEGIN
    CREATE TABLE TaxHub.CompaniesHousePreparation
    (
        TenantReference uniqueidentifier NOT NULL,
        Reference nvarchar(64) NOT NULL,
        CompanyIdentitySha256 char(64) NOT NULL,
        PeriodEnd date NOT NULL,
        CreatedAtUtc datetime2(7) NOT NULL,
        PayloadJson nvarchar(max) NOT NULL,
        CONSTRAINT PK_CompaniesHousePreparation PRIMARY KEY (TenantReference, Reference),
        CONSTRAINT CK_CompaniesHousePreparation_PayloadJson CHECK (ISJSON(PayloadJson) = 1)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'TaxHub.CompaniesHousePreparation')
    AND name = N'IX_CompaniesHousePreparation_CompanyPeriod')
    CREATE INDEX IX_CompaniesHousePreparation_CompanyPeriod
        ON TaxHub.CompaniesHousePreparation (TenantReference, CompanyIdentitySha256, PeriodEnd);
GO

IF OBJECT_ID(N'TaxHub.CompaniesHouseApproval', N'U') IS NULL
BEGIN
    CREATE TABLE TaxHub.CompaniesHouseApproval
    (
        TenantReference uniqueidentifier NOT NULL,
        Reference nvarchar(64) NOT NULL,
        PreparationReference nvarchar(64) NOT NULL,
        LogicalFilingIdentity char(64) NOT NULL,
        CompanyIdentitySha256 char(64) NOT NULL,
        PeriodEnd date NOT NULL,
        ApprovedAtUtc datetime2(7) NOT NULL,
        PayloadJson nvarchar(max) NOT NULL,
        CONSTRAINT PK_CompaniesHouseApproval PRIMARY KEY (TenantReference, Reference),
        CONSTRAINT FK_CompaniesHouseApproval_Preparation FOREIGN KEY (TenantReference, PreparationReference)
            REFERENCES TaxHub.CompaniesHousePreparation (TenantReference, Reference),
        CONSTRAINT CK_CompaniesHouseApproval_PayloadJson CHECK (ISJSON(PayloadJson) = 1)
    );
END;
GO

IF OBJECT_ID(N'TaxHub.CompaniesHouseConversation', N'U') IS NULL
BEGIN
    CREATE TABLE TaxHub.CompaniesHouseConversation
    (
        TenantReference uniqueidentifier NOT NULL,
        Reference nvarchar(64) NOT NULL,
        ApprovalReference nvarchar(64) NOT NULL,
        LogicalFilingIdentity char(64) NOT NULL,
        CompanyIdentitySha256 char(64) NOT NULL,
        State smallint NOT NULL,
        IsActive bit NOT NULL,
        UpdatedAtUtc datetime2(7) NOT NULL,
        PayloadJson nvarchar(max) NOT NULL,
        CONSTRAINT PK_CompaniesHouseConversation PRIMARY KEY (TenantReference, Reference),
        CONSTRAINT FK_CompaniesHouseConversation_Approval FOREIGN KEY (TenantReference, ApprovalReference)
            REFERENCES TaxHub.CompaniesHouseApproval (TenantReference, Reference),
        CONSTRAINT CK_CompaniesHouseConversation_PayloadJson CHECK (ISJSON(PayloadJson) = 1)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'TaxHub.CompaniesHouseApproval')
    AND name = N'UX_CompaniesHouseApproval_Preparation')
    CREATE UNIQUE INDEX UX_CompaniesHouseApproval_Preparation
        ON TaxHub.CompaniesHouseApproval (TenantReference, PreparationReference);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'TaxHub.CompaniesHouseConversation')
    AND name = N'UX_CompaniesHouseConversation_ActiveLogicalFiling')
    CREATE UNIQUE INDEX UX_CompaniesHouseConversation_ActiveLogicalFiling
        ON TaxHub.CompaniesHouseConversation (TenantReference, LogicalFilingIdentity)
        WHERE IsActive = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'TaxHub.CompaniesHouseConversation')
    AND name = N'IX_CompaniesHouseConversation_History')
    CREATE INDEX IX_CompaniesHouseConversation_History
        ON TaxHub.CompaniesHouseConversation (TenantReference, CompanyIdentitySha256, UpdatedAtUtc DESC);
GO
