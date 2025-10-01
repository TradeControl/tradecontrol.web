CREATE TABLE [Org].[tbType] (
    [OrganisationTypeCode] SMALLINT      CONSTRAINT [DF_Org_tbType_OrganisationTypeCode] DEFAULT ((1)) NOT NULL,
    [CashModeCode]         SMALLINT      NOT NULL,
    [OrganisationType]     NVARCHAR (50) NOT NULL,
    [RowVer]               ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Org_tbType] PRIMARY KEY NONCLUSTERED ([OrganisationTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbType_Cash_tbMode] FOREIGN KEY ([CashModeCode]) REFERENCES [Cash].[tbMode] ([CashModeCode])
);

