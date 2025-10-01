CREATE TABLE [Task].[tbAllocation] (
    [ContractAddress]       NVARCHAR (42)   NOT NULL,
    [AccountCode]           NVARCHAR (10)   NOT NULL,
    [AllocationCode]        NVARCHAR (50)   NOT NULL,
    [AllocationDescription] NVARCHAR (256)  NULL,
    [TaskCode]              NVARCHAR (20)   NOT NULL,
    [TaskTitle]             NVARCHAR (100)  NULL,
    [CashModeCode]          SMALLINT        NOT NULL,
    [UnitOfMeasure]         NVARCHAR (15)   NULL,
    [UnitOfCharge]          NVARCHAR (5)    NULL,
    [TaskStatusCode]        SMALLINT        NOT NULL,
    [ActionOn]              DATETIME        NOT NULL,
    [TaxRate]               DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]       DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered]     DECIMAL (18, 4) NOT NULL,
    [InsertedOn]            DATETIME        CONSTRAINT [DF_Task_tbAllocation_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]                ROWVERSION      NOT NULL,
    [UnitCharge]            DECIMAL (18, 7) CONSTRAINT [DF_Task_tbAllocation_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Task_tbAllocation] PRIMARY KEY CLUSTERED ([ContractAddress] ASC),
    CONSTRAINT [FK_Task_tbAllocation_AccountCode] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Task_tbAllocation_CashModeCode] FOREIGN KEY ([CashModeCode]) REFERENCES [Cash].[tbMode] ([CashModeCode]),
    CONSTRAINT [FK_Task_tbAllocation_TaskStatusCode] FOREIGN KEY ([TaskStatusCode]) REFERENCES [Task].[tbStatus] ([TaskStatusCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAllocation_ActivityCode]
    ON [Task].[tbAllocation]([AccountCode] ASC, [AllocationCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAllocation_TaskStatusCode]
    ON [Task].[tbAllocation]([TaskStatusCode] ASC, [AccountCode] ASC, [AllocationCode] ASC, [ActionOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAllocation_TaskCode]
    ON [Task].[tbAllocation]([AccountCode] ASC, [TaskCode] ASC);


GO
CREATE   TRIGGER [Task].Task_tbAllocation_Insert
ON Task.tbAllocation
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Task.tbAllocationEvent (ContractAddress, EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
		SELECT ContractAddress, 2 EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered
		FROM inserted
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER [Task].Task_tbAllocation_Trigger_Update
ON Task.tbAllocation
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF UPDATE(TaskStatusCode)
		BEGIN
			INSERT INTO Task.tbAllocationEvent (ContractAddress, EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 6 EventTypeCode, i.TaskStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.TaskStatusCode <> i.TaskStatusCode
		END

		IF UPDATE(UnitCharge) OR UPDATE(TaxRate)
		BEGIN
			INSERT INTO Task.tbAllocationEvent (ContractAddress, EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 3 EventTypeCode, i.TaskStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.UnitCharge <> i.UnitCharge OR d.TaxRate <> i.TaxRate
		END

		IF UPDATE(ActionOn) OR UPDATE(QuantityOrdered)
		BEGIN
			INSERT INTO Task.tbAllocationEvent (ContractAddress, EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 4 EventTypeCode, i.TaskStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.ActionOn <> i.ActionOn OR d.QuantityOrdered <> i.QuantityOrdered
		END

		IF UPDATE(QuantityDelivered)
		BEGIN
			INSERT INTO Task.tbAllocationEvent (ContractAddress, EventTypeCode, TaskStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 5 EventTypeCode, i.TaskStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.QuantityDelivered <> i.QuantityDelivered
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
