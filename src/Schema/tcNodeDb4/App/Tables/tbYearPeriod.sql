CREATE TABLE [App].[tbYearPeriod] (
    [YearNumber]         SMALLINT        NOT NULL,
    [StartOn]            DATETIME        NOT NULL,
    [MonthNumber]        SMALLINT        NOT NULL,
    [CashStatusCode]     SMALLINT        CONSTRAINT [DF_App_tbYearPeriod_CashStatusCode] DEFAULT ((1)) NOT NULL,
    [InsertedBy]         NVARCHAR (50)   CONSTRAINT [DF_App_tbYearPeriod_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]         DATETIME        CONSTRAINT [DF_App_tbYearPeriod_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [CorporationTaxRate] REAL            CONSTRAINT [DF_App_tbYearPeriod_CorporationTaxRate] DEFAULT ((0)) NOT NULL,
    [RowVer]             ROWVERSION      NOT NULL,
    [TaxAdjustment]      DECIMAL (18, 5) CONSTRAINT [DF_App_tbYearPeriod_TaxAdjustment] DEFAULT ((0)) NOT NULL,
    [VatAdjustment]      DECIMAL (18, 5) CONSTRAINT [DF_App_tbYearPeriod_VatAdjustment] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_App_tbYearPeriod] PRIMARY KEY CLUSTERED ([YearNumber] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbYearPeriod_App_tbMonth] FOREIGN KEY ([MonthNumber]) REFERENCES [App].[tbMonth] ([MonthNumber]),
    CONSTRAINT [FK_App_tbYearPeriod_App_tbYear] FOREIGN KEY ([YearNumber]) REFERENCES [App].[tbYear] ([YearNumber]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_App_tbYearPeriod_Cash_tbStatus] FOREIGN KEY ([CashStatusCode]) REFERENCES [Cash].[tbStatus] ([CashStatusCode]),
    CONSTRAINT [IX_App_tbYearPeriod_StartOn] UNIQUE NONCLUSTERED ([StartOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_App_tbYearPeriod_Year_MonthNumber] UNIQUE NONCLUSTERED ([YearNumber] ASC, [MonthNumber] ASC) WITH (FILLFACTOR = 90)
);

