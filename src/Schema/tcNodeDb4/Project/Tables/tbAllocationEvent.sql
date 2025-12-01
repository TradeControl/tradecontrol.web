CREATE TABLE [Project].[tbAllocationEvent] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [LogId]             INT             IDENTITY (1, 1) NOT NULL,
    [EventTypeCode]     SMALLINT        NOT NULL,
    [ProjectStatusCode]    SMALLINT        NOT NULL,
    [ActionOn]          DATETIME        NOT NULL,
    [TaxRate]           DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]   DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered] DECIMAL (18, 4) NOT NULL,
    [InsertedOn]        DATETIME        CONSTRAINT [DF_Project_tbAllocationEvent_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [UnitCharge]        DECIMAL (18, 7) CONSTRAINT [DF_tbAllocationEvent_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Project_tbAllocationEvent] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [LogId] ASC),
    CONSTRAINT [FK_Project_tbAllocationEvent_App_tbEventType] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode]),
    CONSTRAINT [FK_Project_tbAllocationEvent_Project_tbStatus] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode]),
    CONSTRAINT [FK_Project_tbAllocationEvent_tbAllocation] FOREIGN KEY ([ContractAddress]) REFERENCES [Project].[tbAllocation] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocationEvent_EventTypeCide]
    ON [Project].[tbAllocationEvent]([EventTypeCode] ASC, [ProjectStatusCode] ASC, [InsertedOn] DESC);

