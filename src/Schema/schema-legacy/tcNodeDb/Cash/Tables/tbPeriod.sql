CREATE TABLE [Cash].[tbPeriod] (
    [CashCode]      NVARCHAR (50)   NOT NULL,
    [StartOn]       DATETIME        NOT NULL,
    [Note]          NTEXT           NULL,
    [RowVer]        ROWVERSION      NOT NULL,
    [InvoiceValue]  DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPeriod_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [InvoiceTax]    DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPeriod_InvoiceTax] DEFAULT ((0)) NOT NULL,
    [ForecastValue] DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPeriod_ForecastValue] DEFAULT ((0)) NOT NULL,
    [ForecastTax]   DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPeriod_ForecastTax] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Cash_tbPeriod] PRIMARY KEY CLUSTERED ([CashCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbPeriod_App_tbYearPeriod] FOREIGN KEY ([StartOn]) REFERENCES [App].[tbYearPeriod] ([StartOn]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Cash_tbPeriod_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE   TRIGGER Cash.Cash_tbPeriod_Trigger_Update 
ON Cash.tbPeriod FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
	IF UPDATE (ForecastValue)
		BEGIN
		UPDATE tbPeriod
		SET ForecastTax = inserted.ForecastValue * tax_code.TaxRate
		FROM inserted 
			JOIN Cash.tbPeriod tbPeriod ON inserted.CashCode = tbPeriod.CashCode AND inserted.StartOn = tbPeriod.StartOn
			JOIN Cash.tbCode cash_code ON tbPeriod.CashCode = cash_code.CashCode 
			JOIN Cash.tbCategory ON cash_code.CategoryCode = Cash.tbCategory.CategoryCode 
            JOIN App.tbTaxCode tax_code ON cash_code.TaxCode = tax_code.TaxCode
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
