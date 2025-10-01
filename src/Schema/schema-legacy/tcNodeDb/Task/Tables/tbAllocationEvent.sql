CREATE TABLE [Task].[tbAllocationEvent] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [LogId]             INT             IDENTITY (1, 1) NOT NULL,
    [EventTypeCode]     SMALLINT        NOT NULL,
    [TaskStatusCode]    SMALLINT        NOT NULL,
    [ActionOn]          DATETIME        NOT NULL,
    [TaxRate]           DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]   DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered] DECIMAL (18, 4) NOT NULL,
    [InsertedOn]        DATETIME        CONSTRAINT [DF_Task_tbAllocationEvent_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [UnitCharge]        DECIMAL (18, 7) CONSTRAINT [DF_tbAllocationEvent_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Task_tbAllocationEvent] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [LogId] ASC),
    CONSTRAINT [FK_Task_tbAllocationEvent_App_tbEventType] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode]),
    CONSTRAINT [FK_Task_tbAllocationEvent_Task_tbStatus] FOREIGN KEY ([TaskStatusCode]) REFERENCES [Task].[tbStatus] ([TaskStatusCode]),
    CONSTRAINT [FK_Task_tbAllocationEvent_tbAllocation] FOREIGN KEY ([ContractAddress]) REFERENCES [Task].[tbAllocation] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAllocationEvent_EventTypeCide]
    ON [Task].[tbAllocationEvent]([EventTypeCode] ASC, [TaskStatusCode] ASC, [InsertedOn] DESC);

