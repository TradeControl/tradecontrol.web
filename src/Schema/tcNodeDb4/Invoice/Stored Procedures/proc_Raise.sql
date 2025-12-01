
CREATE   PROCEDURE Invoice.proc_Raise
	(
	@ProjectCode nvarchar(20),
	@InvoiceTypeCode smallint,
	@InvoicedOn datetime,
	@InvoiceNumber nvarchar(20) = null output
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @SubjectCode nvarchar(10)
	
		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END

		SET @InvoicedOn = isnull(CAST(@InvoicedOn AS DATE), CAST(CURRENT_TIMESTAMP AS DATE))
		SELECT @SubjectCode = SubjectCode FROM Project.tbProject WHERE ProjectCode = @ProjectCode


		BEGIN TRANSACTION
	
		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
							(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, Project.tbProject.SubjectCode, @InvoiceTypeCode AS InvoiceTypeCode, @InvoicedOn AS InvoicedOn, 
							0 AS InvoiceStatusCode, Subject.tbSubject.PaymentTerms
		FROM         Project.tbProject INNER JOIN
							Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)

		EXEC Invoice.proc_AddProject @InvoiceNumber, @ProjectCode
	
		IF @@TRANCOUNT > 0		
			COMMIT TRANSACTION
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
