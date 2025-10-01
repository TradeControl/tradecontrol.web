CREATE TABLE [Invoice].[tbMirror] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [InvoiceNumber]     NVARCHAR (50)   NOT NULL,
    [InvoiceTypeCode]   SMALLINT        NOT NULL,
    [InvoiceStatusCode] SMALLINT        NOT NULL,
    [InvoicedOn]        DATETIME        NOT NULL,
    [DueOn]             DATETIME        NOT NULL,
    [UnitOfCharge]      NVARCHAR (5)    NULL,
    [PaymentTerms]      NVARCHAR (100)  NULL,
    [InsertedOn]        DATETIME        CONSTRAINT [DF_Invoice_tbMirror_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirror_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [InvoiceTax]        DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirror_InvoiceTax] DEFAULT ((0)) NOT NULL,
    [PaidValue]         DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirror_PaidValue] DEFAULT ((0)) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirror_PaidTaxValue] DEFAULT ((0)) NOT NULL,
    [PaymentAddress]    NVARCHAR (42)   NULL,
    CONSTRAINT [PK_Invoice_tbMirror] PRIMARY KEY CLUSTERED ([ContractAddress] ASC),
    CONSTRAINT [FK_Invoice_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_Invoice_tbMirror_tbStatus] FOREIGN KEY ([InvoiceStatusCode]) REFERENCES [Invoice].[tbStatus] ([InvoiceStatusCode]),
    CONSTRAINT [FK_Invoice_tbMirror_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirror_InvoiceNumber]
    ON [Invoice].[tbMirror]([SubjectCode] ASC, [InvoiceNumber] ASC);


GO
CREATE   TRIGGER Invoice.Invoice_tbMirror_TriggerInsert
ON Invoice.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
		SELECT ContractAddress, 2 EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue
		FROM inserted;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO

CREATE TRIGGER Invoice.Invoice_tbMirror_TriggerUpdate
ON Invoice.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF UPDATE(InvoiceStatusCode)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 6 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.InvoiceStatusCode <> i.InvoiceStatusCode;	
		END

		IF UPDATE(DueOn)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 4 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.DueOn <> i.DueOn;
		END

		IF UPDATE(PaidValue) OR UPDATE(PaidTaxValue)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 7 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE (d.PaidValue + d.PaidTaxValue) <> (i.PaidValue + i.PaidTaxValue);
		END

		IF UPDATE(PaymentAddress)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue, PaymentAddress)
			SELECT i.ContractAddress, 8 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue, i.PaymentAddress
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.PaymentAddress <> i.PaymentAddress;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
