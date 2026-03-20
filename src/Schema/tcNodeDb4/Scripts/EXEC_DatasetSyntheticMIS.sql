SET NOCOUNT ON;
SET XACT_ABORT ON;

EXEC App.proc_DatasetSyntheticMIS
	@IsCompany = 1,
	@IsVatRegistered = NULL,
	@MisOrdersPerMonth = 2,
	@MonthsForward = 3,
    @PriceRatio = 3,
    @QuantityRatio = 10,
	@FloatRatio = 0.25,
	@EnableProjects = 1,
	@EnableInvoices = 1,
	@EnableProjectPayments = 1,
	@EnablePayables = 1,
	@EnableMiscPayments = 1,
	@EnableWages = 1,
	@EnableExpenses = 1,
    @EnableAssets = 1,
    @EnableTax = 1,
	@EnableTransfers = 1;
