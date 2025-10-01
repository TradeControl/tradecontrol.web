CREATE TABLE [Invoice].[tbMirrorEvent] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [LogId]             INT             IDENTITY (1, 1) NOT NULL,
    [EventTypeCode]     SMALLINT        NULL,
    [InvoiceStatusCode] SMALLINT        NULL,
    [DueOn]             DATETIME        NULL,
    [InsertedOn]        DATETIME        CONSTRAINT [DF_Task_tbMirrorEvent_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [PaidValue]         DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorEvent_PaidValue] DEFAULT ((0)) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorEvent_PaidTaxValue] DEFAULT ((0)) NOT NULL,
    [PaymentAddress]    NVARCHAR (42)   NULL,
    CONSTRAINT [PK_Invoice_tbMirrorEvent] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [LogId] ASC),
    CONSTRAINT [FK_Invoice_tbMirrorEvent_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]),
    CONSTRAINT [FK_Invoice_tbMirrorEvent_EventTypeCode] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirrorEvent_EventTypeCode]
    ON [Invoice].[tbMirrorEvent]([EventTypeCode] ASC, [InvoiceStatusCode] ASC, [InsertedOn] ASC);

