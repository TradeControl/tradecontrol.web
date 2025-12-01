CREATE TABLE [Invoice].[tbProject] (
    [InvoiceNumber] NVARCHAR (20)   NOT NULL,
    [ProjectCode]      NVARCHAR (20)   NOT NULL,
    [CashCode]      NVARCHAR (50)   NOT NULL,
    [TaxCode]       NVARCHAR (10)   NULL,
    [RowVer]        ROWVERSION      NOT NULL,
    [Quantity]      DECIMAL (18, 4) CONSTRAINT [DF_Invoice_tbProject_Quantity] DEFAULT ((0)) NOT NULL,
    [TotalValue]    DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbProject_TotalValue] DEFAULT ((0)) NOT NULL,
    [InvoiceValue]  DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbProject_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [TaxValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbProject_TaxValue] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice_tbProject] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC, [ProjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Invoice_tbProject_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]),
    CONSTRAINT [FK_Invoice_tbProject_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Invoice_tbProject_Invoice_tb] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Invoice_tbProject_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_CashCode]
    ON [Invoice].[tbProject]([CashCode] ASC, [InvoiceNumber] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_Full]
    ON [Invoice].[tbProject]([InvoiceNumber] ASC, [CashCode] ASC, [InvoiceValue] ASC, [TaxValue] ASC, [TaxCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_InvoiceNumber_TaxCode]
    ON [Invoice].[tbProject]([InvoiceNumber] ASC, [TaxCode] ASC)
    INCLUDE([CashCode], [InvoiceValue], [TaxValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_ProjectCode]
    ON [Invoice].[tbProject]([ProjectCode] ASC, [InvoiceNumber] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_TaxCode]
    ON [Invoice].[tbProject]([TaxCode] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


GO
CREATE   TRIGGER Invoice.Invoice_tbProject_TriggerDelete
ON Invoice.tbProject
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE Project.tbProject
		SET ProjectStatusCode = 2
		FROM deleted JOIN Project.tbProject ON deleted.ProjectCode = Project.tbProject.ProjectCode
		WHERE ProjectStatusCode = 3;		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE TRIGGER Invoice.Invoice_tbProject_TriggerInsert
ON Invoice.tbProject
FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE Project
		SET InvoiceValue = inserted.TotalValue / (1 + TaxRate),
			TaxValue = inserted.TotalValue - inserted.TotalValue / (1 + TaxRate)
		FROM inserted 
			INNER JOIN Invoice.tbProject Project ON inserted.InvoiceNumber = Project.InvoiceNumber 
					AND inserted.ProjectCode = Project.ProjectCode
				INNER JOIN App.tbTaxCode ON inserted.TaxCode = App.tbTaxCode.TaxCode 
		WHERE inserted.TotalValue <> 0;

		UPDATE Project
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Project.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND(Project.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) 
			END
		FROM Invoice.tbProject Project 
			INNER JOIN inserted ON inserted.InvoiceNumber = Project.InvoiceNumber
					 AND inserted.ProjectCode = Project.ProjectCode
				INNER JOIN App.tbTaxCode ON Project.TaxCode = App.tbTaxCode.TaxCode
		WHERE inserted.TotalValue = 0; 

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
