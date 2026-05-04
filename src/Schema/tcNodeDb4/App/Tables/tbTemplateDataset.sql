CREATE TABLE [App].[tbTemplateDataset] (
    [DatasetCode]            NVARCHAR (10)   NOT NULL,
    [TemplateCode]           NVARCHAR (10)   NOT NULL,
    [DatasetTitle]           NVARCHAR (100)  NOT NULL,
    [Notes]                  NVARCHAR (MAX)  NULL,
    [IsCompany]              BIT             CONSTRAINT [DF_App_tbTemplateDataset_IsCompany] DEFAULT ((1)) NOT NULL,
    [IsVatRegistered]        BIT             NULL,
    [UseStdCompanyTemplate]  BIT             CONSTRAINT [DF_App_tbTemplateDataset_UseStdCompanyTemplate] DEFAULT ((0)) NOT NULL,
    [MisOrdersPerMonth]      INT             CONSTRAINT [DF_App_tbTemplateDataset_MisOrdersPerMonth] DEFAULT ((2)) NOT NULL,
    [MonthsForward]          INT             CONSTRAINT [DF_App_tbTemplateDataset_MonthsForward] DEFAULT ((3)) NOT NULL,
    [PriceRatio]             DECIMAL (18, 7) CONSTRAINT [DF_App_tbTemplateDataset_PriceRatio] DEFAULT ((1.0000000)) NOT NULL,
    [QuantityRatio]          DECIMAL (18, 7) CONSTRAINT [DF_App_tbTemplateDataset_QuantityRatio] DEFAULT ((1.0000000)) NOT NULL,
    [FloatRatio]             DECIMAL (18, 7) CONSTRAINT [DF_App_tbTemplateDataset_FloatRatio] DEFAULT ((0.25)) NOT NULL,
    CONSTRAINT [PK_App_tbTemplateDataset] PRIMARY KEY CLUSTERED ([DatasetCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbTemplateDataset_App_tbTemplate]
        FOREIGN KEY ([TemplateCode]) REFERENCES [App].[tbTemplate] ([TemplateCode]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_App_tbTemplateDataset_TemplateCode]
    ON [App].[tbTemplateDataset]([TemplateCode] ASC) WITH (FILLFACTOR = 90);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_App_tbTemplateDataset_TemplateCode_DatasetTitle]
    ON [App].[tbTemplateDataset]([TemplateCode] ASC, [DatasetTitle] ASC) WITH (FILLFACTOR = 90);
GO
