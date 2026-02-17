CREATE TABLE [App].[tbOptions] (
    [Identifier]         NVARCHAR (4)   NOT NULL,
    [IsInitialised]      BIT            CONSTRAINT [DF_App_tbOptions_IsIntialised] DEFAULT ((0)) NOT NULL,
    [SubjectCode]        NVARCHAR (10)  NOT NULL,
    [RegisterName]       NVARCHAR (50)  NOT NULL,
    [DefaultPrintMode]   SMALLINT       CONSTRAINT [DF_App_tbOptions_DefaultPrintMode] DEFAULT ((2)) NOT NULL,
    [BucketTypeCode]     SMALLINT       CONSTRAINT [DF_App_tbOptions_BucketTypeCode] DEFAULT ((1)) NOT NULL,
    [BucketIntervalCode] SMALLINT       CONSTRAINT [DF_App_tbOptions_BucketIntervalCode] DEFAULT ((1)) NOT NULL,
    [NetProfitCode]      NVARCHAR (10)  NULL,
    [VatCategoryCode]    NVARCHAR (10)  NULL,
    [TaxHorizon]         SMALLINT       CONSTRAINT [DF_App_tbOptions_TaxHorizon] DEFAULT ((90)) NOT NULL,
    [IsAutoOffsetDays]   BIT            CONSTRAINT [DF_App_tbOptions_IsAutoOffsetDays] DEFAULT ((0)) NOT NULL,
    [UnitOfCharge]       NVARCHAR (5)   NULL,
    [MinerFeeCode]       NVARCHAR (50)  NULL,
    [MinerAccountCode]   NVARCHAR (10)  NULL,
    [CoinTypeCode]       SMALLINT       CONSTRAINT [DF_App_tbOptions_CoinTypeCode] DEFAULT ((2)) NOT NULL,
    [HostId]             INT            NULL,
    [SymmetricKey]       VARBINARY (32) NULL,
    [SymmetricIV]        VARBINARY (16) NULL,
    [SupportRequestTemplateId]      INT NULL,
    [UserRegistrationTemplateId]    INT NULL,
    [UserRegistrationConfirmTemplateId]     INT NULL,
    [UserRegistrationAdminNotifyTemplateId] INT NULL,
    [InsertedBy]         NVARCHAR (50)  CONSTRAINT [DF_App_tbOptions_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]         DATETIME       CONSTRAINT [DF_App_tbOptions_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]          NVARCHAR (50)  CONSTRAINT [DF_App_tbOptions_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]          DATETIME       CONSTRAINT [DF_App_tbOptions_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]             ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbOptions] PRIMARY KEY CLUSTERED ([Identifier] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbOption_Cash_tbCategory] FOREIGN KEY ([NetProfitCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]),
    CONSTRAINT [FK_App_tbOptions_App_tbBucketInterval] FOREIGN KEY ([BucketIntervalCode]) REFERENCES [App].[tbBucketInterval] ([BucketIntervalCode]),
    CONSTRAINT [FK_App_tbOptions_App_tbBucketType] FOREIGN KEY ([BucketTypeCode]) REFERENCES [App].[tbBucketType] ([BucketTypeCode]),
    CONSTRAINT [FK_App_tbOptions_App_tbHost] FOREIGN KEY ([HostId]) REFERENCES [App].[tbHost] ([HostId]),
    CONSTRAINT [FK_App_tbOptions_App_tbRegister] FOREIGN KEY ([RegisterName]) REFERENCES [App].[tbRegister] ([RegisterName]) ON UPDATE CASCADE,
    CONSTRAINT [FK_App_tbOptions_Cash_tbCode] FOREIGN KEY ([MinerFeeCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_App_tbOptions_Cash_tbCoinType] FOREIGN KEY ([CoinTypeCode]) REFERENCES [Cash].[tbCoinType] ([CoinTypeCode]),
    CONSTRAINT [FK_App_tbOptions_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_App_tbOptions_Subject_tbSubject] FOREIGN KEY ([MinerAccountCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_App_tbUoc_UnitOfCharge] FOREIGN KEY ([UnitOfCharge]) REFERENCES [App].[tbUoc] ([UnitOfCharge]),
    CONSTRAINT [FK_App_tbOptions_Web_tbTemplate_SupportRequest]
        FOREIGN KEY ([SupportRequestTemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]),
    CONSTRAINT [FK_App_tbOptions_Web_tbTemplate_UserRegistration]
        FOREIGN KEY ([UserRegistrationTemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]),
    CONSTRAINT [FK_App_tbOptions_Web_tbTemplate_UserRegistrationConfirm]
        FOREIGN KEY ([UserRegistrationConfirmTemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]),
    CONSTRAINT [FK_App_tbOptions_Web_tbTemplate_UserRegistrationAdminNotify]
        FOREIGN KEY ([UserRegistrationAdminNotifyTemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId])

);
GO
CREATE TRIGGER App.App_tbOptions_TriggerUpdate 
   ON App.tbOptions
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE App.tbOptions
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM App.tbOptions INNER JOIN inserted AS i ON tbOptions.Identifier = i.Identifier;

		IF UPDATE(CoinTypeCode)
		BEGIN
			UPDATE Subject.tbAccount
			SET CoinTypeCode = (SELECT CoinTypeCode FROM inserted)
		END

        DECLARE @Candidates TABLE (Code nvarchar(10) NOT NULL PRIMARY KEY);

        INSERT INTO @Candidates(Code)
        SELECT i.NetProfitCode FROM inserted i WHERE i.NetProfitCode IS NOT NULL
        UNION
        SELECT i.VatCategoryCode FROM inserted i WHERE i.VatCategoryCode IS NOT NULL;

        -- Rule 1: must be an enabled category
        IF EXISTS
        (
            SELECT 1
            FROM @Candidates x
            LEFT JOIN Cash.tbCategory c ON c.CategoryCode = x.Code
            WHERE c.CategoryCode IS NULL OR c.IsEnabled = 0
        )
        BEGIN
            RAISERROR('Primary root must be an enabled category.', 16, 1);
            RETURN;
        END

        -- Rule 2: must be a true root (no parent edge)
        IF EXISTS
        (
            SELECT 1
            FROM @Candidates x
            JOIN Cash.tbCategoryTotal t ON t.ChildCode = x.Code
        )
        BEGIN
            RAISERROR('Only root categories (no parents) can be assigned as primary roots.', 16, 1);
            RETURN;
        END

        -- Rule 3: must participate as a parent in the graph (has at least one child)
        IF EXISTS
        (
            SELECT 1
            FROM @Candidates x
            WHERE NOT EXISTS (SELECT 1 FROM Cash.tbCategoryTotal t WHERE t.ParentCode = x.Code)
        )
        BEGIN
            RAISERROR('Primary root must be a parent in the totals graph (has at least one child).', 16, 1);
            RETURN;
        END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
