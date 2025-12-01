CREATE   PROCEDURE Invoice.proc_Mirror(@ContractAddress nvarchar(42), @InvoiceNumber nvarchar(20) OUTPUT)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @SubjectCode nvarchar(10)
		, @InvoiceTypeCode smallint
	
		SELECT @UserId = UserId FROM Usr.vwCredentials
		SET @InvoiceSuffix = '.' + @UserId

		SELECT 
			@InvoiceTypeCode = CASE InvoiceTypeCode 
								WHEN 0 THEN 2
								WHEN 1 THEN 3
								WHEN 2 THEN 0
								WHEN 3 THEN 1
							END
		FROM Invoice.tbMirror
		WHERE ContractAddress = @ContractAddress
	
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

		BEGIN TRAN

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
							(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, DueOn, ExpectedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, mirror.SubjectCode, 
				@InvoiceTypeCode AS InvoiceTypeCode, CAST(mirror.InvoicedOn AS DATE) AS InvoicedOn, mirror.DueOn, mirror.DueOn ExpectedOn, 0 AS InvoiceStatusCode, 
				CASE WHEN Subject.tbSubject.PaymentTerms IS NULL THEN mirror.PaymentTerms ELSE Subject.tbSubject.PaymentTerms END PaymentTerms
		FROM Invoice.tbMirror mirror
			JOIN Subject.tbSubject ON mirror.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE ContractAddress = @ContractAddress;

		INSERT INTO Invoice.tbMirrorReference (ContractAddress, InvoiceNumber)
		VALUES (@ContractAddress, @InvoiceNumber);

		WITH allocations AS
		(
			SELECT 0 Allocationid, 
				allocation.ProjectCode,
				Object_mirror.ObjectCode, allocation.SubjectCode, 
					CASE allocation.CashPolarityCode 
						WHEN 0 THEN Project_mirror.Quantity * -1
						WHEN 1 THEN Project_mirror.Quantity
					END Quantity, allocation.CashPolarityCode
			FROM Invoice.tbMirror invoice_mirror
				JOIN Invoice.tbMirrorProject Project_mirror ON invoice_mirror.ContractAddress = Project_mirror.ContractAddress
				JOIN Project.tbAllocation allocation ON invoice_mirror.SubjectCode = allocation.SubjectCode AND Project_mirror.ProjectCode = allocation.ProjectCode			
				JOIN Object.tbMirror Object_mirror ON invoice_mirror.SubjectCode = Object_mirror.SubjectCode AND allocation.AllocationCode = Object_mirror.AllocationCode
			WHERE invoice_mirror.ContractAddress = @ContractAddress
		), Projects AS
		(
			SELECT ROW_NUMBER() OVER (PARTITION BY Projects.SubjectCode, Projects.ObjectCode ORDER BY ActionOn) Allocationid,
				Projects.ProjectCode, Projects.ObjectCode, Projects.SubjectCode, Projects.Quantity, category.CashPolarityCode
			FROM allocations
				JOIN Project.tbProject Projects ON Projects.ObjectCode = allocations.ObjectCode AND Projects.SubjectCode = allocations.SubjectCode
				JOIN Cash.tbCode cash_code ON Projects.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
			WHERE Projects.ProjectStatusCode BETWEEN 1 AND 2
		), order_book AS
		(
			SELECT Projects.ProjectCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
			FROM Projects
				OUTER APPLY 
					(
						SELECT CASE invoice.InvoiceTypeCode 
									WHEN 1 THEN delivery.Quantity * -1 
									WHEN 3 THEN delivery.Quantity * -1 
									ELSE delivery.Quantity 
								END Quantity
						FROM Invoice.tbProject delivery 
							JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
						WHERE delivery.ProjectCode = Projects.ProjectCode
					) invoice_quantity
			GROUP BY Projects.ProjectCode
		), deliveries AS
		(
			SELECT Allocationid, Projects.ProjectCode, ObjectCode, SubjectCode,
						CASE CashPolarityCode 
							WHEN 0 THEN (Projects.Quantity - order_book.InvoiceQuantity) * -1
							WHEN 1 THEN Projects.Quantity - order_book.InvoiceQuantity
						END Quantity, CashPolarityCode		
			FROM Projects
				JOIN order_book ON Projects.ProjectCode = order_book.ProjectCode
		), svd_union AS
		(
			SELECT * FROM deliveries
			UNION 
			SELECT * FROM allocations
		), svd_projected AS
		(
			SELECT *,
				SUM(Quantity) OVER (PARTITION BY SubjectCode, ObjectCode  ORDER BY AllocationId ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
			FROM svd_union
		), svd_balance AS
		(
			SELECT *, LAG(Balance) OVER (PARTITION BY SubjectCode, ObjectCode  ORDER BY AllocationId) PreviousBalance
			FROM svd_projected
		), alloc_deliveries AS
		(
			SELECT *, 
					CASE CashPolarityCode 
						WHEN 0 THEN
							CASE WHEN Balance > 0 THEN ABS(Quantity) 
								WHEN PreviousBalance > 0 THEN PreviousBalance
								ELSE 0
							END 
						WHEN 1 THEN
							CASE WHEN Balance < 0 THEN Quantity
								WHEN PreviousBalance < 0 THEN ABS(PreviousBalance)
								ELSE 0
							END 
					END QuantityDelivered
			FROM svd_balance
		)
		INSERT INTO Invoice.tbProject (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT @InvoiceNumber InvoiceNumber, alloc_deliveries.ProjectCode, alloc_deliveries.QuantityDelivered, Project.UnitCharge * alloc_deliveries.QuantityDelivered, Project.CashCode, Project.TaxCode 
		FROM alloc_deliveries
			JOIN Project.tbProject Project ON alloc_deliveries.ProjectCode = Project.ProjectCode
		WHERE QuantityDelivered > 0;

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, InvoiceValue, ItemReference)
		SELECT @InvoiceNumber InvoiceNumber, cash_code_mirror.CashCode, 
			CASE WHEN (item_mirror.TaxValue / item_mirror.InvoiceValue) <> tax_code.TaxRate 
				THEN (SELECT TOP 1 TaxCode FROM App.tbTaxCode WHERE TaxTypeCode = 1 AND ROUND(TaxRate, 3) =  ROUND((item_mirror.TaxValue / item_mirror.InvoiceValue), 3))
				ELSE tax_code.TaxCode 
				END TaxCode,
				item_mirror.InvoiceValue, item_mirror.ChargeDescription ItemReference
		FROM Invoice.tbMirror invoice_mirror 
			JOIN Invoice.tbMirrorItem item_mirror ON invoice_mirror.ContractAddress = item_mirror.ContractAddress			
			JOIN Cash.tbMirror cash_code_mirror ON item_mirror.ChargeCode = cash_code_mirror.ChargeCode and invoice_mirror.SubjectCode = cash_code_mirror.SubjectCode
			JOIN Cash.tbCode cash_code ON cash_code_mirror.CashCode = cash_code.CashCode
			JOIN App.tbTaxCode tax_code ON cash_code.TaxCode = tax_code.TaxCode
		WHERE invoice_mirror.ContractAddress = @ContractAddress

		EXEC Invoice.proc_Total @InvoiceNumber	

		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
