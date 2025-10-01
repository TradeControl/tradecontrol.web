CREATE   PROCEDURE Invoice.proc_Mirror(@ContractAddress nvarchar(42), @InvoiceNumber nvarchar(20) OUTPUT)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @AccountCode nvarchar(10)
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
							(InvoiceNumber, UserId, AccountCode, InvoiceTypeCode, InvoicedOn, DueOn, ExpectedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, mirror.AccountCode, 
				@InvoiceTypeCode AS InvoiceTypeCode, CAST(mirror.InvoicedOn AS DATE) AS InvoicedOn, mirror.DueOn, mirror.DueOn ExpectedOn, 0 AS InvoiceStatusCode, 
				CASE WHEN Org.tbOrg.PaymentTerms IS NULL THEN mirror.PaymentTerms ELSE Org.tbOrg.PaymentTerms END PaymentTerms
		FROM Invoice.tbMirror mirror
			JOIN Org.tbOrg ON mirror.AccountCode = Org.tbOrg.AccountCode
		WHERE ContractAddress = @ContractAddress;

		INSERT INTO Invoice.tbMirrorReference (ContractAddress, InvoiceNumber)
		VALUES (@ContractAddress, @InvoiceNumber);

		WITH allocations AS
		(
			SELECT 0 Allocationid, 
				allocation.TaskCode,
				activity_mirror.ActivityCode, allocation.AccountCode, 
					CASE allocation.CashModeCode 
						WHEN 0 THEN task_mirror.Quantity * -1
						WHEN 1 THEN task_mirror.Quantity
					END Quantity, allocation.CashModeCode
			FROM Invoice.tbMirror invoice_mirror
				JOIN Invoice.tbMirrorTask task_mirror ON invoice_mirror.ContractAddress = task_mirror.ContractAddress
				JOIN Task.tbAllocation allocation ON invoice_mirror.AccountCode = allocation.AccountCode AND task_mirror.TaskCode = allocation.TaskCode			
				JOIN Activity.tbMirror activity_mirror ON invoice_mirror.AccountCode = activity_mirror.AccountCode AND allocation.AllocationCode = activity_mirror.AllocationCode
			WHERE invoice_mirror.ContractAddress = @ContractAddress
		), tasks AS
		(
			SELECT ROW_NUMBER() OVER (PARTITION BY tasks.AccountCode, tasks.ActivityCode ORDER BY ActionOn) Allocationid,
				tasks.TaskCode, tasks.ActivityCode, tasks.AccountCode, tasks.Quantity, category.CashModeCode
			FROM allocations
				JOIN Task.tbTask tasks ON tasks.ActivityCode = allocations.ActivityCode AND tasks.AccountCode = allocations.AccountCode
				JOIN Cash.tbCode cash_code ON tasks.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
			WHERE tasks.TaskStatusCode BETWEEN 1 AND 2
		), order_book AS
		(
			SELECT tasks.TaskCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
			FROM tasks
				OUTER APPLY 
					(
						SELECT CASE invoice.InvoiceTypeCode 
									WHEN 1 THEN delivery.Quantity * -1 
									WHEN 3 THEN delivery.Quantity * -1 
									ELSE delivery.Quantity 
								END Quantity
						FROM Invoice.tbTask delivery 
							JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
						WHERE delivery.TaskCode = tasks.TaskCode
					) invoice_quantity
			GROUP BY tasks.TaskCode
		), deliveries AS
		(
			SELECT Allocationid, tasks.TaskCode, ActivityCode, AccountCode,
						CASE CashModeCode 
							WHEN 0 THEN (tasks.Quantity - order_book.InvoiceQuantity) * -1
							WHEN 1 THEN tasks.Quantity - order_book.InvoiceQuantity
						END Quantity, CashModeCode		
			FROM tasks
				JOIN order_book ON tasks.TaskCode = order_book.TaskCode
		), svd_union AS
		(
			SELECT * FROM deliveries
			UNION 
			SELECT * FROM allocations
		), svd_projected AS
		(
			SELECT *,
				SUM(Quantity) OVER (PARTITION BY AccountCode, ActivityCode  ORDER BY AllocationId ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
			FROM svd_union
		), svd_balance AS
		(
			SELECT *, LAG(Balance) OVER (PARTITION BY AccountCode, ActivityCode  ORDER BY AllocationId) PreviousBalance
			FROM svd_projected
		), alloc_deliveries AS
		(
			SELECT *, 
					CASE CashModeCode 
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
		INSERT INTO Invoice.tbTask (InvoiceNumber, TaskCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT @InvoiceNumber InvoiceNumber, alloc_deliveries.TaskCode, alloc_deliveries.QuantityDelivered, task.UnitCharge * alloc_deliveries.QuantityDelivered, task.CashCode, task.TaxCode 
		FROM alloc_deliveries
			JOIN Task.tbTask task ON alloc_deliveries.TaskCode = task.TaskCode
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
			JOIN Cash.tbMirror cash_code_mirror ON item_mirror.ChargeCode = cash_code_mirror.ChargeCode and invoice_mirror.AccountCode = cash_code_mirror.AccountCode
			JOIN Cash.tbCode cash_code ON cash_code_mirror.CashCode = cash_code.CashCode
			JOIN App.tbTaxCode tax_code ON cash_code.TaxCode = tax_code.TaxCode
		WHERE invoice_mirror.ContractAddress = @ContractAddress

		EXEC Invoice.proc_Total @InvoiceNumber	

		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
