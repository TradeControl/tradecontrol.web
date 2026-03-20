CREATE PROCEDURE App.proc_DatasetSyntheticMIS
(
	@IsCompany bit = 1,
	@IsVatRegistered bit = NULL,

	@MisOrdersPerMonth int = 2,
	@MonthsForward int = 3,

    -- ratios
    @PriceRatio decimal(18,7) = 1.0000000,
    @QuantityRatio decimal(18,7) = 1.0000000,
	@FloatRatio decimal(18,7) = 0.25,

	-- execution switches
	@EnableProjects bit = 1,
	@EnableInvoices bit = 1,
	@EnableProjectPayments bit = 1,
	@EnablePayables bit = 1,
	@EnableMiscPayments bit = 1,
	@EnableWages bit = 1,
	@EnableExpenses bit = 1,
    @EnableAssets bit = 1,
    @EnableTax bit = 1,
	@EnableTransfers bit = 1
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN;

		DECLARE @TemplateName nvarchar(100) =
			CASE WHEN @IsCompany = 1
				THEN N'Minimal Micro Company Accounts 2026'
				ELSE N'Sole Trader Accounts 2026'
			END;

		-- If caller didn't specify VAT setting, inherit from template default.
		IF @IsVatRegistered IS NULL
		BEGIN
			SELECT @IsVatRegistered = IsVatRegistered
			FROM App.tbTemplate
			WHERE TemplateName = @TemplateName;
		END

		---------------------------------------------------------------------
		-- Shared lookup table for the whole run (visible to nested procs)
		---------------------------------------------------------------------
		IF OBJECT_ID('tempdb..#DatasetCodes') IS NOT NULL
			DROP TABLE #DatasetCodes;

		CREATE TABLE #DatasetCodes
		(
			CodeType nvarchar(20) NOT NULL,        -- 'SUBJECT' | 'OBJECT' | 'LINK' | 'PROJECT' etc
			CodeName nvarchar(100) NOT NULL,       -- logical name (unique within CodeType)
			CodeValue nvarchar(50) NOT NULL,       -- SubjectCode (10) or ObjectCode (50) or ProjectCode (20) etc
			RelatedName nvarchar(100) NULL,
			Notes nvarchar(255) NULL,
			PRIMARY KEY (CodeType, CodeName)
		);

		---------------------------------------------------------------------
		-- 1) Bootstrap (ALWAYS resets node inside the proc)
		---------------------------------------------------------------------
        EXEC App.proc_DatasetSyntheticMIS_Bootstrap
	        @TemplateName = @TemplateName,
	        @IsVatRegistered = @IsVatRegistered;

		---------------------------------------------------------------------
		-- 2) Project side
		---------------------------------------------------------------------
		IF @EnableProjects <> 0
		BEGIN
			EXEC App.proc_DatasetSyntheticMIS_ProjectInit
				@IsCompany = @IsCompany,
				@IsVatRegistered = @IsVatRegistered;

			EXEC App.proc_DatasetSyntheticMIS_ProjectTemplates
				@IsCompany = @IsCompany,
				@IsVatRegistered = @IsVatRegistered,
                @PriceRatio = @PriceRatio,
                @QuantityRatio = @QuantityRatio;

			EXEC App.proc_DatasetSyntheticMIS_ProjectTran
				@IsCompany = @IsCompany,
				@IsVatRegistered = @IsVatRegistered,
				@MisOrdersPerMonth = @MisOrdersPerMonth,
				@MonthsForward = @MonthsForward;

		    IF @EnableInvoices <> 0
		    BEGIN
			    EXEC App.proc_DatasetSyntheticMIS_ProjectInvoice
				    @IsCompany = @IsCompany,
				    @IsVatRegistered = @IsVatRegistered;
		    END

		    IF @EnableProjectPayments <> 0 
		    BEGIN
			    EXEC App.proc_DatasetSyntheticMIS_ProjectPay
				    @IsCompany = @IsCompany,
				    @IsVatRegistered = @IsVatRegistered;
		    END
		END

		---------------------------------------------------------------------
		-- 3) Payables side (Accounts mode)
		---------------------------------------------------------------------
		IF @EnablePayables <> 0
		BEGIN
			EXEC App.proc_DatasetSyntheticMIS_PayInit
				@IsCompany = @IsCompany,
				@IsVatRegistered = @IsVatRegistered;

		    IF @EnableMiscPayments <> 0
		    BEGIN
			    EXEC App.proc_DatasetSyntheticMIS_PayMisc
				    @IsCompany = @IsCompany,
				    @IsVatRegistered = @IsVatRegistered;
		    END

		    -- company-only behavior (typical): wages/NI should not run for sole trader
		    IF @EnableWages <> 0 AND @IsCompany <> 0
		    BEGIN
			    EXEC App.proc_DatasetSyntheticMIS_PayWages
				    @IsCompany = @IsCompany,
				    @IsVatRegistered = @IsVatRegistered;
		    END

		    IF @EnableExpenses <> 0
		    BEGIN
			    EXEC App.proc_DatasetSyntheticMIS_Expenses
				    @IsCompany = @IsCompany,
				    @IsVatRegistered = @IsVatRegistered;
		    END
		END

		---------------------------------------------------------------------
		-- 3) Company Assets
		---------------------------------------------------------------------        
        IF @IsCompany <> 0 AND @EnableAssets <> 0
        BEGIN
	        EXEC App.proc_DatasetSyntheticMIS_Assets
		        @IsCompany = @IsCompany,
		        @IsVatRegistered = @IsVatRegistered;
        END

		---------------------------------------------------------------------
		-- 4) Tax
		---------------------------------------------------------------------        
        IF @IsVatRegistered != 0 AND @EnableTax != 0
        BEGIN
            EXEC App.proc_DatasetSyntheticMIS_TaxVat
		        @IsCompany = @IsCompany,
		        @IsVatRegistered = @IsVatRegistered;
        END

		---------------------------------------------------------------------
		-- 5) Transfers (Current -> Reserve sweep)
		---------------------------------------------------------------------
		IF @EnableTransfers <> 0
		BEGIN
			EXEC App.proc_DatasetSyntheticMIS_Transfers
				@FloatRatio = @FloatRatio;
		END

		---------------------------------------------------------------------
		-- 6) Corporation Tax (annual; paid from Reserve via transfer to Current)
		---------------------------------------------------------------------
		IF @IsCompany <> 0 AND @EnableTax <> 0
		BEGIN
			EXEC App.proc_DatasetSyntheticMIS_TaxOnProfit
				@IsCompany = @IsCompany;
		END

		EXEC App.proc_SystemRebuild;

		COMMIT TRAN;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
