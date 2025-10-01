CREATE TABLE [Project].[tbAllocation] (
    [ContractAddress]       NVARCHAR (42)   NOT NULL,
    [SubjectCode]           NVARCHAR (10)   NOT NULL,
    [AllocationCode]        NVARCHAR (50)   NOT NULL,
    [AllocationDescription] NVARCHAR (256)  NULL,
    [ProjectCode]              NVARCHAR (20)   NOT NULL,
    [ProjectTitle]             NVARCHAR (100)  NULL,
    [CashPolarityCode]          SMALLINT        NOT NULL,
    [UnitOfMeasure]         NVARCHAR (15)   NULL,
    [UnitOfCharge]          NVARCHAR (5)    NULL,
    [ProjectStatusCode]        SMALLINT        NOT NULL,
    [ActionOn]              DATETIME        NOT NULL,
    [TaxRate]               DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]       DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered]     DECIMAL (18, 4) NOT NULL,
    [InsertedOn]            DATETIME        CONSTRAINT [DF_Project_tbAllocation_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]                ROWVERSION      NOT NULL,
    [UnitCharge]            DECIMAL (18, 7) CONSTRAINT [DF_Project_tbAllocation_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Project_tbAllocation] PRIMARY KEY CLUSTERED ([ContractAddress] ASC),
    CONSTRAINT [FK_Project_tbAllocation_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tbAllocation_CashPolarityCode] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]),
    CONSTRAINT [FK_Project_tbAllocation_ProjectStatusCode] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ObjectCode]
    ON [Project].[tbAllocation]([SubjectCode] ASC, [AllocationCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ProjectStatusCode]
    ON [Project].[tbAllocation]([ProjectStatusCode] ASC, [SubjectCode] ASC, [AllocationCode] ASC, [ActionOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ProjectCode]
    ON [Project].[tbAllocation]([SubjectCode] ASC, [ProjectCode] ASC);


GO
CREATE   TRIGGER [Project].Project_tbAllocation_Insert
ON Project.tbAllocation
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
		SELECT ContractAddress, 2 EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered
		FROM inserted
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER [Project].Project_tbAllocation_Trigger_Update
ON Project.tbAllocation
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF UPDATE(ProjectStatusCode)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 6 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.ProjectStatusCode <> i.ProjectStatusCode
		END

		IF UPDATE(UnitCharge) OR UPDATE(TaxRate)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 3 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.UnitCharge <> i.UnitCharge OR d.TaxRate <> i.TaxRate
		END

		IF UPDATE(ActionOn) OR UPDATE(QuantityOrdered)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 4 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.ActionOn <> i.ActionOn OR d.QuantityOrdered <> i.QuantityOrdered
		END

		IF UPDATE(QuantityDelivered)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 5 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.QuantityDelivered <> i.QuantityDelivered
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
