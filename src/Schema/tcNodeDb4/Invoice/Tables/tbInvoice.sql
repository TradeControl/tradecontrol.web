CREATE TABLE [Invoice].[tbInvoice] (
    [InvoiceNumber]     NVARCHAR (20)   NOT NULL,
    [UserId]            NVARCHAR (10)   NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [InvoiceTypeCode]   SMALLINT        NOT NULL,
    [InvoiceStatusCode] SMALLINT        NOT NULL,
    [InvoicedOn]        DATETIME        CONSTRAINT [DF_Invoice_tb_InvoicedOn] DEFAULT (CONVERT([date],getdate())) NOT NULL,
    [ExpectedOn]        DATETIME        CONSTRAINT [DF_Invoice_tbInvoice_ExpectedOn] DEFAULT (dateadd(day,(1),CONVERT([date],getdate()))) NOT NULL,
    [DueOn]             DATETIME        CONSTRAINT [DF_Invoice_tbInvoice_DueOn] DEFAULT (dateadd(day,(1),CONVERT([date],getdate()))) NOT NULL,
    [PaymentTerms]      NVARCHAR (100)  NULL,
    [Notes]             NTEXT           NULL,
    [Printed]           BIT             CONSTRAINT [DF_Invoice_tb_Printed] DEFAULT ((0)) NOT NULL,
    [Spooled]           BIT             CONSTRAINT [DF_Invoice_tb_Spooled] DEFAULT ((0)) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tb_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [TaxValue]          DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tb_TaxValue] DEFAULT ((0)) NOT NULL,
    [PaidValue]         DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tb_PaidValue] DEFAULT ((0)) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tb_PaidTaxValue] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice_tbInvoicePK] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Invoice_tb_Invoice_tbStatus] FOREIGN KEY ([InvoiceStatusCode]) REFERENCES [Invoice].[tbStatus] ([InvoiceStatusCode]),
    CONSTRAINT [FK_Invoice_tb_Invoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]),
    CONSTRAINT [FK_Invoice_tb_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_Invoice_tb_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_AccountCode]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoicedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_Status]
    ON [Invoice].[tbInvoice]([InvoiceStatusCode] ASC, [InvoicedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_UserId]
    ON [Invoice].[tbInvoice]([UserId] ASC, [InvoiceNumber] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_DueOn]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceTypeCode] ASC, [DueOn] ASC)
    INCLUDE([InvoiceNumber]);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_Status]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceStatusCode] ASC, [InvoiceNumber] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_Type]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceNumber] ASC, [InvoiceTypeCode] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountValues]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceStatusCode] ASC, [InvoiceNumber] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_ExpectedOn]
    ON [Invoice].[tbInvoice]([ExpectedOn] ASC, [InvoiceTypeCode] ASC, [InvoiceStatusCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_FlowInitialise]
    ON [Invoice].[tbInvoice]([InvoiceTypeCode] ASC, [UserId] ASC, [InvoiceStatusCode] ASC, [SubjectCode] ASC, [InvoiceNumber] ASC, [InvoicedOn] ASC, [PaymentTerms] ASC, [Printed] ASC);


GO
CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerDelete
ON Invoice.tbInvoice
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF EXISTS (SELECT * FROM deleted INNER JOIN Subject.tbSubject ON deleted.SubjectCode = Subject.tbSubject.SubjectCode WHERE Subject.tbSubject.TransmitStatusCode > 1)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 1220;
			RAISERROR (@Msg, 10, 1)
		END
		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO

CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerUpdate
ON Invoice.tbInvoice
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF UPDATE(Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT     App.fnDocInvoiceType(i.InvoiceTypeCode) AS DocTypeCode, i.InvoiceNumber
			FROM         inserted i 
			WHERE     (i.Spooled <> 0)

			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.InvoiceNumber = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode > 3)
		END


		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.DueOn = deleted.DueOn)
		BEGIN
			UPDATE invoice
			SET DueOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
				WHERE i.InvoiceTypeCode = 0;
		END;	

		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.ExpectedOn = deleted.ExpectedOn)
		BEGIN
			UPDATE invoice
			SET ExpectedOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
				WHERE i.InvoiceTypeCode = 0;
		END;	
		
		WITH invoices AS
		(
			SELECT inserted.InvoiceNumber, inserted.SubjectCode, inserted.InvoiceStatusCode, inserted.DueOn, inserted.InvoiceValue, inserted.TaxValue, inserted.PaidValue, inserted.PaidTaxValue FROM inserted JOIN deleted ON inserted.InvoiceNumber = deleted.InvoiceNumber WHERE inserted.InvoiceStatusCode = 1 AND deleted.InvoiceStatusCode = 0
		)
		INSERT INTO Invoice.tbChangeLog (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
		SELECT InvoiceNumber, Subjects.TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue
		FROM invoices JOIN Subject.tbSubject Subjects ON invoices.SubjectCode = Subjects.SubjectCode;

		IF UPDATE(InvoiceStatusCode) OR UPDATE(DueOn) OR UPDATE(PaidValue) OR UPDATE(PaidTaxValue) OR UPDATE(InvoiceValue) OR UPDATE (TaxValue)
		BEGIN
			WITH candidates AS
			(
				SELECT InvoiceNumber, SubjectCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue 
				FROM inserted
				WHERE EXISTS (SELECT * FROM Invoice.tbChangeLog WHERE InvoiceNumber = inserted.InvoiceNumber)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.InvoiceNumber, clog.InvoiceStatusCode, clog.TransmitStatusCode, clog.DueOn, clog.InvoiceValue, clog.TaxValue, clog.PaidValue, clog.PaidTaxValue 
				FROM Invoice.tbChangeLog clog
				JOIN candidates ON clog.InvoiceNumber = candidates.InvoiceNumber AND LogId = (SELECT MAX(LogId) FROM Invoice.tbChangeLog WHERE InvoiceNumber = candidates.InvoiceNumber)		
			)
			INSERT INTO Invoice.tbChangeLog
									 (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
			SELECT candidates.InvoiceNumber, CASE Subjects.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.InvoiceStatusCode,
				candidates.DueOn, candidates.InvoiceValue, candidates.TaxValue, candidates.PaidValue, candidates.PaidTaxValue
			FROM candidates 
				JOIN Subject.tbSubject Subjects ON candidates.SubjectCode = Subjects.SubjectCode 
				JOIN logs ON candidates.InvoiceNumber = logs.InvoiceNumber
			WHERE (logs.InvoiceStatusCode <> candidates.InvoiceStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.DueOn <> candidates.DueOn) 
				OR ((logs.InvoiceValue + logs.TaxValue + logs.PaidValue + logs.PaidTaxValue) 
						<> (candidates.InvoiceValue + candidates.TaxValue + candidates.PaidValue + candidates.PaidTaxValue))
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerInsert
ON Invoice.tbInvoice
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE invoice
		SET DueOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
												THEN 
													DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))												
												ELSE
													DATEADD(d, Subject.PaymentDays, invoice.InvoicedOn)	
												END, 0),
			ExpectedOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
												THEN 
													DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.InvoicedOn), 'yyyyMM'), '01')))												
												ELSE
													DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, invoice.InvoicedOn)	
												END, 0)				 
		FROM Invoice.tbInvoice invoice
			JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
			JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
		WHERE i.InvoiceTypeCode = 0;

		INSERT INTO Invoice.tbChangeLog
								 (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue)
		SELECT      inserted.InvoiceNumber, Subject.tbSubject.TransmitStatusCode, inserted.InvoiceStatusCode, inserted.DueOn, inserted.InvoiceValue, inserted.TaxValue
		FROM            inserted INNER JOIN
								 Subject.tbSubject ON inserted.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE InvoiceStatusCode > 0
								 
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
