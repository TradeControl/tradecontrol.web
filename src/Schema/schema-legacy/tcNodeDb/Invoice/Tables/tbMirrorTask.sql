CREATE TABLE [Invoice].[tbMirrorTask] (
    [ContractAddress] NVARCHAR (42)   NOT NULL,
    [TaskCode]        NVARCHAR (20)   NOT NULL,
    [Quantity]        DECIMAL (18, 4) NOT NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [RowVer]          ROWVERSION      NULL,
    [InvoiceValue]    DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorTask_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [TaxValue]        DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorTask_TaxValue] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorTask] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [TaskCode] ASC),
    CONSTRAINT [FK_Invoice_tbMirrorTask_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbMirrorTask_TaskCode]
    ON [Invoice].[tbMirrorTask]([TaskCode] ASC, [ContractAddress] ASC);


GO
CREATE   TRIGGER Invoice.Invoice_tbMirrorTask_TriggerInsert
ON Invoice.tbMirrorTask
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		WITH deliveries AS
		(
			SELECT mirror.AccountCode, inserted.TaskCode, 
				CASE mirror.InvoiceTypeCode
					WHEN 0 THEN inserted.Quantity
					WHEN 1 THEN inserted.Quantity * -1
					WHEN 2 THEN inserted.Quantity
					WHEN 3 THEN inserted.Quantity * -1
					ELSE 0
				END QuantityDelivered
			FROM inserted
				JOIN Invoice.tbMirror mirror ON inserted.ContractAddress = mirror.ContractAddress
		)
		UPDATE allocs
		SET QuantityDelivered += deliveries.QuantityDelivered
		FROM Task.tbAllocation allocs
			JOIN deliveries ON allocs.AccountCode = deliveries.AccountCode AND allocs.TaskCode = deliveries.TaskCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
