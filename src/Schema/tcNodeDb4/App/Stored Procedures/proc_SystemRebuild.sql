CREATE PROCEDURE App.proc_SystemRebuild
AS
  	SET NOCOUNT, XACT_ABORT ON;

	DECLARE @SubjectCode nvarchar(10), @PaymentCode nvarchar(20);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE Project.tbFlow
		SET UsedOnQuantity = Project.Quantity / parent_Project.Quantity
		FROM            Project.tbFlow AS flow 
			JOIN Project.tbProject AS Project ON flow.ChildProjectCode = Project.ProjectCode 
			JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
			JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
		WHERE        (flow.UsedOnQuantity <> 0) AND (Project.Quantity <> 0) 
			AND (Project.Quantity / parent_Project.Quantity <> flow.UsedOnQuantity);

		WITH parent_Project AS
		(
			SELECT        ParentProjectCode
			FROM            Project.tbFlow flow
				JOIN Project.tbProject Project ON flow.ParentProjectCode = Project.ProjectCode
				JOIN Cash.tbCode cash ON Project.CashCode = cash.CashCode
		), Project_flow AS
		(
			SELECT        flow.ParentProjectCode, flow.StepNumber, Project.ActionOn,
					LAG(Project.ActionOn, 1, Project.ActionOn) OVER (PARTITION BY flow.ParentProjectCode ORDER BY StepNumber) AS PrevActionOn
			FROM Project.tbFlow flow
				JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
				JOIN parent_Project ON flow.ParentProjectCode = parent_Project.ParentProjectCode
		), step_disordered AS
		(
			SELECT ParentProjectCode 
			FROM Project_flow
			WHERE ActionOn < PrevActionOn
			GROUP BY ParentProjectCode
		), step_ordered AS
		(
			SELECT flow.ParentProjectCode, flow.ChildProjectCode,
				ROW_NUMBER() OVER (PARTITION BY flow.ParentProjectCode ORDER BY Project.ActionOn, flow.StepNumber) * 10 AS StepNumber 
			FROM step_disordered
				JOIN Project.tbFlow flow ON step_disordered.ParentProjectCode = flow.ParentProjectCode
				JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
		)
		UPDATE flow
		SET
			StepNumber = step_ordered.StepNumber
		FROM Project.tbFlow flow
			JOIN step_ordered ON flow.ParentProjectCode = step_ordered.ParentProjectCode AND flow.ChildProjectCode = step_ordered.ChildProjectCode;

		--invoices	
		UPDATE Invoice.tbItem
		SET 
			InvoiceValue =  ROUND(Invoice.tbItem.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbItem.TotalValue <> 0;

		UPDATE Invoice.tbItem
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0);
                   
		UPDATE Invoice.tbProject
		SET InvoiceValue =  ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbProject.TotalValue <> 0;

		UPDATE Invoice.tbProject
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END,
			InvoiceValue = CASE WHEN Invoice.tbProject.TotalValue = 0 
								THEN Invoice.tbProject.InvoiceValue 
								ELSE ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals) 
							END
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0);
						   	
	
		WITH items AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbItem.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbItem.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbItem INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), Projects AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbProject.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbProject.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbProject INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), invoice_totals AS
		(
			SELECT invoices.InvoiceNumber, 
				COALESCE(items.TotalInvoiceValue, 0) + COALESCE(Projects.TotalInvoiceValue, 0) TotalInvoiceValue,
				COALESCE(items.TotalTaxValue, 0) + COALESCE(Projects.TotalTaxValue, 0) TotalTaxValue
			FROM Invoice.tbInvoice invoices
				LEFT OUTER JOIN Projects ON invoices.InvoiceNumber = Projects.InvoiceNumber
				LEFT OUTER JOIN items ON invoices.InvoiceNumber = items.InvoiceNumber
			WHERE ( invoices.InvoiceStatusCode > 0)
		)
		UPDATE invoices
		SET InvoiceValue = TotalInvoiceValue, 
			TaxValue = TotalTaxValue
		FROM  Invoice.tbInvoice invoices 
			JOIN invoice_totals ON invoices.InvoiceNumber = invoice_totals.InvoiceNumber
		WHERE (InvoiceValue <> TotalInvoiceValue OR TaxValue <> TotalTaxValue);

		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
		)
		UPDATE invoices
		SET 
			InvoiceStatusCode = invoice_status.InvoiceStatusCode,
			PaidValue = invoice_status.PaidValue,
			PaidTaxValue = invoice_status.PaidTaxValue
		FROM Invoice.tbInvoice invoices	
			JOIN invoice_status ON invoices.InvoiceNumber = invoice_status.InvoiceNumber
		WHERE 
			invoices.InvoiceStatusCode <> invoice_status.InvoiceStatusCode 
			OR invoices.PaidValue <> invoice_status.PaidValue 
			OR invoices.PaidTaxValue <> invoice_status.PaidTaxValue;		
		--cash accounts
		UPDATE Subject.tbAccount
		SET CurrentBalance = Cash.vwAccountRebuild.CurrentBalance
		FROM         Cash.vwAccountRebuild INNER JOIN
							Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode;
	
		UPDATE Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.OpeningBalance
		FROM         Cash.vwAccountRebuild RIGHT OUTER JOIN
							  Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE     (Cash.vwAccountRebuild.AccountCode IS NULL);

		EXEC Cash.proc_GeneratePeriods;
	            
		COMMIT TRANSACTION

		DECLARE @Msg NVARCHAR(MAX);
		SELECT @Msg = Message FROM App.tbText WHERE TextId = 3006;
		EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 2;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
