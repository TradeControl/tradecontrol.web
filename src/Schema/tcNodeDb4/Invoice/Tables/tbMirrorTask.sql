CREATE TABLE [Invoice].[tbMirrorProject] (
    [ContractAddress] NVARCHAR (42)   NOT NULL,
    [ProjectCode]        NVARCHAR (20)   NOT NULL,
    [Quantity]        DECIMAL (18, 4) NOT NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [RowVer]          ROWVERSION      NULL,
    [InvoiceValue]    DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorProject_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [TaxValue]        DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorProject_TaxValue] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorProject] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [ProjectCode] ASC),
    CONSTRAINT [FK_Invoice_tbMirrorProject_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbMirrorProject_ProjectCode]
    ON [Invoice].[tbMirrorProject]([ProjectCode] ASC, [ContractAddress] ASC);


GO
CREATE   TRIGGER Invoice.Invoice_tbMirrorProject_TriggerInsert
ON Invoice.tbMirrorProject
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		WITH deliveries AS
		(
			SELECT mirror.SubjectCode, inserted.ProjectCode, 
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
		FROM Project.tbAllocation allocs
			JOIN deliveries ON allocs.SubjectCode = deliveries.SubjectCode AND allocs.ProjectCode = deliveries.ProjectCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
