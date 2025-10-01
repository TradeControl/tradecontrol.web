CREATE TABLE [Cash].[tbTaxType] (
    [TaxTypeCode]    SMALLINT      NOT NULL,
    [TaxType]        NVARCHAR (20) NOT NULL,
    [CashCode]       NVARCHAR (50) NULL,
    [MonthNumber]    SMALLINT      CONSTRAINT [DF_App_tbOptions_MonthNumber] DEFAULT ((1)) NOT NULL,
    [RecurrenceCode] SMALLINT      CONSTRAINT [DF_App_tbOptions_Recurrence] DEFAULT ((1)) NOT NULL,
    [AccountCode]    NVARCHAR (10) NULL,
    [OffsetDays]     SMALLINT      CONSTRAINT [DF_Cash_tbTaxType_OffsetDays] DEFAULT ((0)) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbTaxType] PRIMARY KEY CLUSTERED ([TaxTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbTaxType_App_tbMonth] FOREIGN KEY ([MonthNumber]) REFERENCES [App].[tbMonth] ([MonthNumber]),
    CONSTRAINT [FK_Cash_tbTaxType_App_tbRecurrence] FOREIGN KEY ([RecurrenceCode]) REFERENCES [App].[tbRecurrence] ([RecurrenceCode]),
    CONSTRAINT [FK_Cash_tbTaxType_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Cash_tbTaxType_Org_tb] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_tbTaxType_CashCode]
    ON [Cash].[tbTaxType]([CashCode] ASC);

