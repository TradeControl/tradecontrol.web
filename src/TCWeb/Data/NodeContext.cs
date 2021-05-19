using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;

using TradeControl.Web.Models;

#nullable disable

namespace TradeControl.Web.Data
{
    public partial class NodeContext : IdentityDbContext<TradeControlWebUser>
    {
        public static string NodeVersion { get; }  = "3.34.2";

        public NodeContext(DbContextOptions<NodeContext> options) : base(options) { }

        #region Tables
        public virtual DbSet<Org_tbAccount> Org_tbAccounts { get; set; }
        public virtual DbSet<Org_tbAccountType> Org_tbAccountTypes { get; set; }
        public virtual DbSet<Activity_tbActivity> Activity_tbActivities { get; set; }
        public virtual DbSet<Org_tbAddress> Org_tbAddresses { get; set; }
        public virtual DbSet<Task_tbAllocation> Task_tbAllocations { get; set; }
        public virtual DbSet<Task_tbAllocationEvent> Task_tbAllocationEvents { get; set; }
        public virtual DbSet<Cash_tbAssetType> Cash_tbAssetTypes { get; set; }
        public virtual DbSet<Activity_tbAttribute> Activity_tbAttributes { get; set; }
        public virtual DbSet<Task_tbAttribute> Task_tbAttributes { get; set; }
        public virtual DbSet<Activity_tbAttributeType> Activity_tbAttributeTypes { get; set; }
        public virtual DbSet<App_tbBucket> App_tbBuckets { get; set; }
        public virtual DbSet<App_tbBucketInterval> App_tbBucketIntervals { get; set; }
        public virtual DbSet<App_tbBucketType> App_tbBucketTypes { get; set; }
        public virtual DbSet<App_tbCalendar> App_tbCalendars { get; set; }
        public virtual DbSet<App_tbCalendarHoliday> App_tbCalendarHolidays { get; set; }
        public virtual DbSet<Cash_tbCategory> Cash_tbCategories { get; set; }
        public virtual DbSet<Cash_tbCategoryExp> Cash_tbCategoryExps { get; set; }
        public virtual DbSet<Cash_tbCategoryTotal> Cash_tbCategoryTotals { get; set; }
        public virtual DbSet<Cash_tbCategoryType> Cash_tbCategoryTypes { get; set; }
        public virtual DbSet<Cash_tbChange> Cash_tbChanges { get; set; }
        public virtual DbSet<Invoice_tbChangeLog> Invoice_tbChangeLogs { get; set; }
        public virtual DbSet<Task_tbChangeLog> Task_tbChangeLogs { get; set; }
        public virtual DbSet<Cash_tbChangeReference> Cash_tbChangeReferences { get; set; }
        public virtual DbSet<Cash_tbChangeStatus> Cash_tbChangeStatuses { get; set; }
        public virtual DbSet<Cash_tbChangeType> Cash_tbChangeTypes { get; set; }
        public virtual DbSet<Cash_tbCode> Cash_tbCodes { get; set; }
        public virtual DbSet<App_tbCodeExclusion> App_tbCodeExclusions { get; set; }
        public virtual DbSet<Cash_tbCoinType> Cash_tbCoinTypes { get; set; }
        public virtual DbSet<Org_tbContact> Org_tbContacts { get; set; }
        public virtual DbSet<Task_tbCostSet> Task_tbCostSets { get; set; }
        public virtual DbSet<App_tbDoc> App_tbDocs { get; set; }
        public virtual DbSet<Org_tbDoc> Org_tbDocs { get; set; }
        public virtual DbSet<Task_tbDoc> Task_tbDocs { get; set; }
        public virtual DbSet<App_tbDocClass> App_tbDocClasses { get; set; }
        public virtual DbSet<App_tbDocSpool> App_tbDocSpools { get; set; }
        public virtual DbSet<App_tbDocType> App_tbDocTypes { get; set; }
        public virtual DbSet<Invoice_tbEntry> Invoice_tbEntries { get; set; }
        public virtual DbSet<Cash_tbEntryType> Cash_tbEntryTypes { get; set; }
        public virtual DbSet<App_tbEth> App_tbEths { get; set; }
        public virtual DbSet<App_tbEventLog> App_tbEventLogs { get; set; }
        public virtual DbSet<App_tbEventType> App_tbEventTypes { get; set; }
        public virtual DbSet<Activity_tbFlow> Activity_tbFlows { get; set; }
        public virtual DbSet<Task_tbFlow> Task_tbFlows { get; set; }
        public virtual DbSet<App_tbInstall> App_tbInstalls { get; set; }
        public virtual DbSet<Usr_tbInterface> Usr_tbInterfaces { get; set; }
        public virtual DbSet<Invoice_tbInvoice> Invoice_tbInvoices { get; set; }
        public virtual DbSet<Invoice_tbItem> Invoice_tbItems { get; set; }
        public virtual DbSet<Usr_tbMenu> Usr_tbMenus { get; set; }
        public virtual DbSet<Usr_tbMenuCommand> Usr_tbMenuCommands { get; set; }
        public virtual DbSet<Usr_tbMenuEntry> Usr_tbMenuEntries { get; set; }
        public virtual DbSet<Usr_tbMenuOpenMode> Usr_tbMenuOpenModes { get; set; }
        public virtual DbSet<Usr_tbMenuUser> Usr_tbMenuUsers { get; set; }
        public virtual DbSet<Usr_tbMenuView> Usr_tbMenuViews { get; set; }
        public virtual DbSet<Activity_tbMirror> Activity_tbMirrors { get; set; }
        public virtual DbSet<Cash_tbMirror> Cash_tbMirrors { get; set; }
        public virtual DbSet<Invoice_tbMirror> Invoice_tbMirrors { get; set; }
        public virtual DbSet<Invoice_tbMirrorEvent> Invoice_tbMirrorEvents { get; set; }
        public virtual DbSet<Invoice_tbMirrorItem> Invoice_tbMirrorItems { get; set; }
        public virtual DbSet<Invoice_tbMirrorReference> Invoice_tbMirrorReferences { get; set; }
        public virtual DbSet<Invoice_tbMirrorTask> Invoice_tbMirrorTasks { get; set; }
        public virtual DbSet<Cash_tbMode> Cash_tbModes { get; set; }
        public virtual DbSet<App_tbMonth> App_tbMonths { get; set; }
        public virtual DbSet<Activity_tbOp> Activity_tbOps { get; set; }
        public virtual DbSet<Task_tbOp> Task_tbOps { get; set; }
        public virtual DbSet<Task_tbOpStatus> Task_tbOpStatuses { get; set; }
        public virtual DbSet<App_tbOption> App_tbOptions { get; set; }
        public virtual DbSet<Org_tbOrg> Org_tbOrgs { get; set; }
        public virtual DbSet<Cash_tbPayment> Cash_tbPayments { get; set; }
        public virtual DbSet<Cash_tbPaymentStatus> Cash_tbPaymentStatuses { get; set; }
        public virtual DbSet<App_tbPeriod> App_tbPeriods { get; set; }
        public virtual DbSet<Task_tbQuote> Task_tbQuotes { get; set; }
        public virtual DbSet<App_tbRecurrence> App_tbRecurrences { get; set; }
        public virtual DbSet<App_tbRegister> App_tbRegisters { get; set; }
        public virtual DbSet<App_tbRounding> App_tbRoundings { get; set; }
        public virtual DbSet<Org_tbSector> Org_tbSectors { get; set; }
        public virtual DbSet<Cash_tbStatus> Cash_tbStatuses { get; set; }
        public virtual DbSet<Invoice_tbStatus> Invoice_tbStatuses { get; set; }
        public virtual DbSet<Org_tbStatus> Org_tbStatuses { get; set; }
        public virtual DbSet<Task_tbStatus> Task_tbStatuses { get; set; }
        public virtual DbSet<Activity_tbSyncType> Activity_tbSyncTypes { get; set; }
        public virtual DbSet<Invoice_tbTask> Invoice_tbTasks { get; set; }
        public virtual DbSet<Task_tbTask> Task_tbTasks { get; set; }
        public virtual DbSet<App_tbTaxCode> App_tbTaxCodes { get; set; }
        public virtual DbSet<Cash_tbTaxType> Cash_tbTaxTypes { get; set; }
        public virtual DbSet<App_tbText> App_tbTexts { get; set; }
        public virtual DbSet<Org_tbTransmitStatus> Org_tbTransmitStatuses { get; set; }
        public virtual DbSet<Cash_tbTx> Cash_tbTxs { get; set; }
        public virtual DbSet<Cash_tbTxReference> Cash_tbTxReferences { get; set; }
        public virtual DbSet<Cash_tbTxStatus> Cash_tbTxStatuses { get; set; }
        public virtual DbSet<Cash_tbType> Cash_tbTypes { get; set; }
        public virtual DbSet<Invoice_tbType> Invoice_tbTypes { get; set; }
        public virtual DbSet<Org_tbType> Org_tbTypes { get; set; }
        public virtual DbSet<App_tbUoc> App_tbUocs { get; set; }
        public virtual DbSet<App_tbUom> App_tbUoms { get; set; }
        public virtual DbSet<Usr_tbUser> Usr_tbUsers { get; set; }
        public virtual DbSet<App_tbYear> App_tbYears { get; set; }
        public virtual DbSet<App_tbYearPeriod> App_tbYearPeriods { get; set; }

        #endregion

        #region Asp.Net
        public virtual DbSet<AspNet_UserRegistration> AspNet_UserRegistrations { get; set; }

        #endregion

        #region Views
        public virtual DbSet<Org_vwAccountLookup> Org_AccountLookup { get; set; }
        public virtual DbSet<Org_vwAccountSource> Org_AccountSources { get; set; }
        public virtual DbSet<Cash_vwAccountStatement> Cash_AccountStatements { get; set; }
        public virtual DbSet<Cash_vwAccountStatementListing> Cash_AccountStatementListings { get; set; }
        public virtual DbSet<Invoice_vwAccountsMode> Invoice_AccountsMode { get; set; }
        public virtual DbSet<Task_vwActiveDatum> Task_ActiveData { get; set; }
        public virtual DbSet<App_vwActivePeriod> App_ActivePeriods { get; set; }
        public virtual DbSet<Task_vwActiveStatusCode> Task_ActiveStatusCodes { get; set; }
        public virtual DbSet<Invoice_vwAgedDebtPurchase> Invoice_AgedDebtPurchases { get; set; }
        public virtual DbSet<Invoice_vwAgedDebtSale> Invoice_AgedDebtSales { get; set; }
        public virtual DbSet<Task_vwAllocationSvD> Task_AllocationSvD { get; set; }
        public virtual DbSet<Org_vwAreaCode> Org_AreaCodes { get; set; }
        public virtual DbSet<Org_vwAssetStatementAudit> Org_AssetStatementAudits { get; set; }
        public virtual DbSet<Task_vwAttributeDescription> Task_AttributeDescriptions { get; set; }
        public virtual DbSet<Task_vwAttributesForOrder> Task_AttributesForOrders { get; set; }
        public virtual DbSet<Task_vwAttributesForQuote> Task_AttributesForQuotes { get; set; }
        public virtual DbSet<Org_vwBalanceSheetAudit> VwBalanceSheetAudits { get; set; }
        public virtual DbSet<Cash_vwBankCashCode> Cash_BankCashCodes { get; set; }
        public virtual DbSet<Cash_vwBudget> Cash_Budget { get; set; }
        public virtual DbSet<Cash_vwBudgetDataEntry> Cash_BudgetDataEntries { get; set; }
        public virtual DbSet<Activity_wCandidateCashCode> VwCandidateCashCodes { get; set; }
        public virtual DbSet<App_vwCandidateCategoryCode> App_CandidateCategoryCodes { get; set; }
        public virtual DbSet<Invoice_vwCandidateCredit> Invoice_CandidateCredits { get; set; }
        public virtual DbSet<Invoice_vwCandidateDebit> Invoice_CandidateDebits { get; set; }
        public virtual DbSet<App_vwCandidateHomeAccount> App_CandidateHomeAccounts { get; set; }
        public virtual DbSet<Invoice_vwCandidatePurchase> Invoice_CandidatePurchases { get; set; }
        public virtual DbSet<Invoice_vwCandidateSale> Invoice_CandidateSales { get; set; }
        public virtual DbSet<Org_vwCashAccountAsset> Org_CashAccountAssets { get; set; }
        public virtual DbSet<Org_vwCashAccount> Org_CashAccounts { get; set; }
        public virtual DbSet<Cash_vwCashFlowType> Cash_CashFlowTypes { get; set; }
        public virtual DbSet<Cash_vwCategoryBudget> Cash_CategoryBudget { get; set; }
        public virtual DbSet<Cash_vwCategoryTotal> Cash_CategoryTotals { get; set; }
        public virtual DbSet<Cash_vwCategoryTotalCandidate> Cash_CategoryTotalCandidates { get; set; }
        public virtual DbSet<Cash_vwCategoryTrade> Cash_CategoryTrades { get; set; }
        public virtual DbSet<Invoice_vwChangeLog> Invoice_ChangeLog { get; set; }
        public virtual DbSet<Task_vwChangeLog> Task_ChangeLog { get; set; }
        public virtual DbSet<Activity_vwCode> Activity_Codes { get; set; }
        public virtual DbSet<Cash_vwCodeLookup> Cash_CodeLookup { get; set; }
        public virtual DbSet<Org_vwCompanyHeader> Org_CompanyHeaders { get; set; }
        public virtual DbSet<Org_vwCompanyLogo> Org_CompanyLogos { get; set; }
        public virtual DbSet<Org_vwContact> Org_Contacts { get; set; }
        public virtual DbSet<Task_vwCostSet> Task_CostSet { get; set; }
        public virtual DbSet<Invoice_vwCreditNoteSpool> Invoice_CreditNoteSpool { get; set; }
        public virtual DbSet<Invoice_vwCreditSpoolByItem> Invoice_CreditSpoolByItem { get; set; }
        public virtual DbSet<Org_vwDatasheet> Org_Datasheet { get; set; }
        public virtual DbSet<Invoice_vwDebitNoteSpool> Invoice_DebitNoteSpool { get; set; }
        public virtual DbSet<Activity_vwDefaultText> Activity_DefaultText { get; set; }
        public virtual DbSet<Org_vwDepartment> Org_Departments { get; set; }
        public virtual DbSet<App_vwDocCreditNote> App_DocCreditNotes { get; set; }
        public virtual DbSet<App_vwDocDebitNote> App_DocDebitNotes { get; set; }
        public virtual DbSet<App_vwDocOpenMode> App_DocOpenModes { get; set; }
        public virtual DbSet<App_vwDocPurchaseEnquiry> App_DocPurchaseEnquiries { get; set; }
        public virtual DbSet<App_vwDocPurchaseOrder> App_DocPurchaseOrders { get; set; }
        public virtual DbSet<App_vwDocQuotation> App_DocQuotations { get; set; }
        public virtual DbSet<App_vwDocSalesInvoice> App_DocSalesInvoices { get; set; }
        public virtual DbSet<App_vwDocSalesOrder> App_DocSalesOrders { get; set; }
        public virtual DbSet<App_vwEventLog> App_EventLogs { get; set; }
        public virtual DbSet<Activity_vwExpenseCashCode> Activity_ExpenseCashCodes { get; set; }
        public virtual DbSet<Cash_vwExternalCodesLookup> Cash_ExternalCodesLookup { get; set; }
        public virtual DbSet<Task_vwFlow> Task_Flow { get; set; }
        public virtual DbSet<App_vwGraphBankBalance> App_GraphBankBalances { get; set; }
        public virtual DbSet<App_vwGraphTaskActivity> App_GraphTaskActivities { get; set; }
        public virtual DbSet<Invoice_vwHistoryCashCode> Invoice_HistoryCashCodes { get; set; }
        public virtual DbSet<Invoice_vwHistoryPurchase> Invoice_HistoryPurchases { get; set; }
        public virtual DbSet<Invoice_vwHistoryPurchaseItem> Invoice_HistoryPurchaseItems { get; set; }
        public virtual DbSet<Invoice_vwHistorySale> Invoice_HistorySales { get; set; }
        public virtual DbSet<Invoice_vwHistorySalesItem> Invoice_HistorySalesItems { get; set; }
        public virtual DbSet<App_vwIdentity> App_Identities { get; set; }
        public virtual DbSet<Activity_wIncomeCashCode> VwIncomeCashCodes { get; set; }
        public virtual DbSet<Org_vwInvoiceItem> Org_InvoiceItems { get; set; }
        public virtual DbSet<Org_vwInvoiceSummary> Org_InvoiceSummaries { get; set; }
        public virtual DbSet<Org_vwInvoiceTask> Org_InvoiceTasks { get; set; }
        public virtual DbSet<Invoice_vwItem> Invoice_Items { get; set; }
        public virtual DbSet<Org_vwJobTitle> Org_JobTitles { get; set; }
        public virtual DbSet<Org_vwListActive> Org_ListActive { get; set; }
        public virtual DbSet<Org_vwListAll> Org_ListAll { get; set; }
        public virtual DbSet<Invoice_vwMirror> Invoice_Mirrors { get; set; }
        public virtual DbSet<Invoice_vwMirrorDetail> Invoice_MirrorDetails { get; set; }
        public virtual DbSet<Invoice_vwMirrorEvent> Invoice_MirrorEvents { get; set; }
        public virtual DbSet<Org_vwNameTitle> Org_NameTitles { get; set; }
        public virtual DbSet<Task_vwNetworkAllocation> Task_NetworkAllocations { get; set; }
        public virtual DbSet<Invoice_vwNetworkChangeLog> Invoice_NetworkChangeLog { get; set; }
        public virtual DbSet<Task_vwNetworkChangeLog> Task_NetworkChangeLogs { get; set; }
        public virtual DbSet<Task_vwNetworkEvent> Task_NetworkEvents { get; set; }
        public virtual DbSet<Task_vwNetworkEventLog> Task_NetworkEventLog { get; set; }
        public virtual DbSet<Task_vwNetworkQuotation> Task_NetworkQuotations { get; set; }
        public virtual DbSet<Task_vwOp> Task_Ops { get; set; }
        public virtual DbSet<Cash_vwPayment> Cash_Payments { get; set; }
        public virtual DbSet<Org_vwPaymentTerm> Org_PaymentTerms { get; set; }
        public virtual DbSet<Cash_vwPaymentsListing> Cash_PaymentsListing { get; set; }
        public virtual DbSet<Cash_vwPaymentsUnposted> Cash_PaymentsUnposted { get; set; }
        public virtual DbSet<App_vwPeriod> App_Periods { get; set; }
        public virtual DbSet<App_vwPeriodEndListing> App_PeriodEndListings { get; set; }
        public virtual DbSet<Task_vwProfit> Task_Profit { get; set; }
        public virtual DbSet<Task_vwProfitToDate> Task_ProfitToDate { get; set; }
        public virtual DbSet<Task_vwPurchase> Task_Purchases { get; set; }
        public virtual DbSet<Task_vwPurchaseEnquiryDeliverySpool> Task_PurchaseEnquiryDeliverySpool { get; set; }
        public virtual DbSet<Task_vwPurchaseEnquirySpool> Task_PurchaseEnquirySpool { get; set; }
        public virtual DbSet<Task_vwPurchaseOrderDeliverySpool> Task_PurchaseOrderDeliverySpool { get; set; }
        public virtual DbSet<Task_vwPurchaseOrderSpool> Task_PurchaseOrderSpool { get; set; }
        public virtual DbSet<Task_vwQuotationSpool> Task_QuotationSpool { get; set; }
        public virtual DbSet<Task_vwQuote> Task_Quotes { get; set; }
        public virtual DbSet<Invoice_vwRegister> Invoice_Register { get; set; }
        public virtual DbSet<Invoice_vwRegisterCashCode> Invoice_RegisterCashCodes { get; set; }
        public virtual DbSet<Invoice_vwRegisterDetail> Invoice_RegisterDetails { get; set; }
        public virtual DbSet<Invoice_vwRegisterExpense> Invoice_RegisterExpenses { get; set; }
        public virtual DbSet<Invoice_vwRegisterItem> Invoice_RegisterItems { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchase> Invoice_RegisterPurchases { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchaseTask> Invoice_RegisterPurchaseTasks { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchasesOverdue> Invoice_RegisterPurchasesOverdue { get; set; }
        public virtual DbSet<Invoice_vwRegisterSale> Invoice_RegisterSales { get; set; }
        public virtual DbSet<Invoice_vwRegisterSaleTask> Invoice_RegisterSaleTasks { get; set; }
        public virtual DbSet<Invoice_vwRegisterSalesOverdue> Invoice_RegisterSalesOverdues { get; set; }
        public virtual DbSet<Task_vwSale> Task_Sales { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpool> Invoice_SalesInvoiceSpool { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpoolByActivity> Invoice_SalesInvoiceSpoolByActivity { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpoolByItem> Invoice_SalesInvoiceSpoolByItem { get; set; }
        public virtual DbSet<Task_vwSalesOrderSpool> Task_SalesOrderSpool { get; set; }
        public virtual DbSet<Cash_vwStatement> Cash_Statement { get; set; }
        public virtual DbSet<Org_wStatement> OrgStatements { get; set; }
        public virtual DbSet<Org_vwStatementReport> Org_StatementReport { get; set; }
        public virtual DbSet<Cash_vwStatementReserve> Cash_StatementReserves { get; set; }
        public virtual DbSet<Cash_vwStatementWhatIf> Cash_StatementWhatIf { get; set; }
        public virtual DbSet<Org_vwStatusReport> Org_StatusReport { get; set; }
        public virtual DbSet<Cash_vwSummary> Cash_Summary { get; set; }
        public virtual DbSet<Invoice_vwSummary> Invoice_Summary { get; set; }
        public virtual DbSet<Org_vwTask> Org_Tasks { get; set; }
        public virtual DbSet<Task_vwTask> Task_Tasks { get; set; }
        public virtual DbSet<App_vwTaxCode> App_TaxCodes { get; set; }
        public virtual DbSet<App_vwTaxCodeType> App_TaxCodeTypes { get; set; }
        public virtual DbSet<Cash_vwTaxCorpAuditAccrual> Cash_TaxCorpAuditAccruals { get; set; }
        public virtual DbSet<Cash_vwTaxCorpStatement> Cash_TaxCorpStatement { get; set; }
        public virtual DbSet<Cash_vwTaxCorpTotal> Cash_TaxCorpTotals { get; set; }
        public virtual DbSet<Invoice_vwTaxSummary> Invoice_TaxSummary { get; set; }
        public virtual DbSet<Cash_vwTaxVatAuditAccrual> Cash_TaxVatAuditAccruals { get; set; }
        public virtual DbSet<Cash_vwTaxVatAuditInvoice> Cash_TaxVatAuditInvoices { get; set; }
        public virtual DbSet<Cash_vwTaxVatDetail> Cash_TaxVatDetails { get; set; }
        public virtual DbSet<Cash_vwTaxVatStatement> Cash_TaxVatStatement { get; set; }
        public virtual DbSet<Cash_vwTaxVatSummary> Cash_TaxVatSummary { get; set; }
        public virtual DbSet<Cash_vwTaxVatTotal> Cash_TaxVatTotals { get; set; }
        public virtual DbSet<Task_vwTitle> Task_Titles { get; set; }
        public virtual DbSet<Cash_vwTransferCodeLookup> Cash_TransferCodeLookup { get; set; }
        public virtual DbSet<Cash_vwTransfersUnposted> Cash_TransfersUnposted { get; set; }
        public virtual DbSet<Org_vwTypeLookup> Org_TypeLookup { get; set; }
        public virtual DbSet<Activity_vwUnMirrored> Activity_UnMirrored { get; set; }
        public virtual DbSet<Cash_vwUnMirrored> Cash_UnMirrored { get; set; }
        public virtual DbSet<Usr_vwUserMenu> Usr_UserMenus { get; set; }
        public virtual DbSet<Usr_vwUserMenuList> Usr_UserMenuLists { get; set; }
        public virtual DbSet<Cash_vwVatcode> Cash_Vatcodes { get; set; }
        public virtual DbSet<App_vwVersion> App_Version { get; set; }
        public virtual DbSet<App_vwHomeAccount> App_HomeAccount { get; set; }
        public virtual DbSet<App_vwWarehouseOrg> App_WarehouseOrgs { get; set; }
        public virtual DbSet<App_vwWarehouseTask> App_WarehouseTasks { get; set; }
        public virtual DbSet<App_vwYearPeriod> App_YearPeriods { get; set; }
        public virtual DbSet<Usr_vwCredential> Usr_Credentials { get; set; }
        #endregion

        #region Model Creation
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            #region Asp.Net
            modelBuilder.Entity<AspNet_UserRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                //entity.ToView("AspNetUserRegistrations");
            });
            #endregion

            modelBuilder.Entity<Org_tbAccount>(entity =>
            {
                entity.HasKey(e => e.CashAccountCode)
                    .HasName("PK_Org_tbAccount");

                entity.HasIndex(e => new { e.AccountCode, e.CashAccountCode }, "IX_Org_tbAccount")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.CoinTypeCode).HasDefaultValueSql("((2))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Org_tbAccount_Org_tb");

                entity.HasOne(d => d.AccountTypeCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.AccountTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Org_tbAccount_Org_tbAccountType");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Org_tbAccount_Cash_tbCode");

                entity.HasOne(d => d.CoinTypeCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.CoinTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Org_tbAccount_Cash_tbCoinType");
            });

            modelBuilder.Entity<Org_tbAccountType>(entity =>
            {
                entity.HasKey(e => e.AccountTypeCode)
                    .HasName("PK_Org_tbAccountType");

                entity.Property(e => e.AccountTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbActivity>(entity =>
            {
                entity.HasKey(e => e.ActivityCode)
                    .HasName("PK_Activity_tbActivityCode")
                    .IsClustered(false);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.TaskStatusCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Activity_tbActivity_Cash_tbCode");

                entity.HasOne(d => d.RegisterNameNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.RegisterName)
                    .HasConstraintName("FK_Activity_tbActivity_App_tbRegister");

                entity.HasOne(d => d.UnitOfMeasureNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.UnitOfMeasure)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbActivity_App_tbUom");
            });

            modelBuilder.Entity<Org_tbAddress>(entity =>
            {
                entity.HasKey(e => e.AddressCode)
                    .HasName("PK_Org_tbAddress");

                entity.HasIndex(e => new { e.AccountCode, e.AddressCode }, "IX_Org_tbAddress")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbAddresses)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Org_tbAddress_Org_tb");
            });

            modelBuilder.Entity<Task_tbAllocation>(entity =>
            {
                entity.HasKey(e => e.ContractAddress)
                    .HasName("PK_Task_tbAllocation");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Task_tbAllocation_AccountCode");

                entity.HasOne(d => d.CashModeCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.CashModeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbAllocation_CashModeCode");

                entity.HasOne(d => d.TaskStatusCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.TaskStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbAllocation_TaskStatusCode");
            });

            modelBuilder.Entity<Task_tbAllocationEvent>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.LogId })
                    .HasName("PK_Task_tbAllocationEvent");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.ContractAddress)
                    .HasConstraintName("FK_Task_tbAllocationEvent_tbAllocation");

                entity.HasOne(d => d.EventTypeCodeNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.EventTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbAllocationEvent_App_tbEventType");

                entity.HasOne(d => d.TaskStatusCodeNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.TaskStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbAllocationEvent_Task_tbStatus");
            });

            modelBuilder.Entity<Cash_tbAssetType>(entity =>
            {
                entity.HasKey(e => e.AssetTypeCode)
                    .HasName("PK_Cash_tbAssetType");

                entity.Property(e => e.AssetTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbAttribute>(entity =>
            {
                entity.HasKey(e => new { e.ActivityCode, e.Attribute })
                    .HasName("PK_Activity_tbAttribute");

                entity.HasIndex(e => e.Attribute, "IX_Activity_tbAttribute")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.DefaultText, "IX_Activity_tbAttribute_DefaultText")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ActivityCode, e.PrintOrder, e.Attribute }, "IX_Activity_tbAttribute_OrderBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ActivityCode, e.AttributeTypeCode, e.PrintOrder }, "IX_Activity_tbAttribute_Type_OrderBy")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrintOrder).HasDefaultValueSql("((10))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ActivityCodeNavigation)
                    .WithMany(p => p.TbAttributes)
                    .HasForeignKey(d => d.ActivityCode)
                    .HasConstraintName("FK_Activity_tbAttribute_tbActivity");

                entity.HasOne(d => d.AttributeTypeCodeNavigation)
                    .WithMany(p => p.TbAttributes)
                    .HasForeignKey(d => d.AttributeTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbAttribute_Activity_tbAttributeType");
            });

            modelBuilder.Entity<Task_tbAttribute>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.Attribute })
                    .HasName("PK_Task_tbTaskAttribute");

                entity.HasIndex(e => e.TaskCode, "IX_Task_tbAttribute")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.Attribute, e.AttributeDescription }, "IX_Task_tbAttribute_Description")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.TaskCode, e.PrintOrder, e.Attribute }, "IX_Task_tbAttribute_OrderBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.TaskCode, e.AttributeTypeCode, e.PrintOrder }, "IX_Task_tbAttribute_Type_OrderBy")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrintOrder).HasDefaultValueSql("((10))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AttributeTypeCodeNavigation)
                    .WithMany(p => p.TbAttribute1s)
                    .HasForeignKey(d => d.AttributeTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbAttribute_Activity_tbAttributeType");

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbAttribute1s)
                    .HasForeignKey(d => d.TaskCode)
                    .HasConstraintName("FK_Task_tbAttrib_Task_tb");
            });

            modelBuilder.Entity<Activity_tbAttributeType>(entity =>
            {
                entity.HasKey(e => e.AttributeTypeCode)
                    .HasName("PK_Activity_tbAttributeType");

                entity.Property(e => e.AttributeTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbBucket>(entity =>
            {
                entity.HasKey(e => e.Period)
                    .HasName("PK_App_tbBucket");

                entity.Property(e => e.Period).ValueGeneratedNever();

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_tbBucketInterval>(entity =>
            {
                entity.HasKey(e => e.BucketIntervalCode)
                    .HasName("PK_App_tbBucketInterval");

                entity.Property(e => e.BucketIntervalCode).ValueGeneratedNever();

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_tbBucketType>(entity =>
            {
                entity.HasKey(e => e.BucketTypeCode)
                    .HasName("PK_App_tbBucketType");

                entity.Property(e => e.BucketTypeCode).ValueGeneratedNever();
            });

            /*
            modelBuilder.Entity<App_tbCalendar>(entity =>
            {

                entity.HasKey(e => e.CalendarCode)
                    .HasName("PK_App_tbCalendar");

                entity.Property(e => e.Friday).HasDefaultValueSql("((1))");

                entity.Property(e => e.Monday).HasDefaultValueSql("((1))");

                entity.Property(e => e.Thursday).HasDefaultValueSql("((1))");

                entity.Property(e => e.Tuesday).HasDefaultValueSql("((1))");

                entity.Property(e => e.Wednesday).HasDefaultValueSql("((1))");

            });
            */

            modelBuilder.Entity<App_tbCalendarHoliday>(entity =>
            {
                entity.HasKey(e => new { e.CalendarCode, e.UnavailableOn })
                    .HasName("PK_App_tbCalendarHoliday");

                entity.HasIndex(e => e.CalendarCode, "IX_App_tbCalendarHoliday_CalendarCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CalendarCodeNavigation)
                    .WithMany(p => p.TbCalendarHolidays)
                    .HasForeignKey(d => d.CalendarCode)
                    .HasConstraintName("App_tbCalendarHoliday_FK00");
            });

            modelBuilder.Entity<Cash_tbCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryCode)
                    .HasName("PK_Cash_tbCategory");

                entity.HasIndex(e => new { e.DisplayOrder, e.Category }, "IX_Cash_tbCategory_DisplayOrder")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.Category, "IX_Cash_tbCategory_Name")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.CategoryTypeCode, e.Category }, "IX_Cash_tbCategory_TypeCategory")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.CategoryTypeCode, e.DisplayOrder, e.Category }, "IX_Cash_tbCategory_TypeOrderCategory")
                    .HasFillFactor((byte)90);

                //entity.Property(e => e.CashModeCode).HasDefaultValueSql("((1))");

                //entity.Property(e => e.CashTypeCode).HasDefaultValueSql("((0))");

                //entity.Property(e => e.CategoryTypeCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsEnabled).HasDefaultValueSql("((1))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CashModeCodeNavigation)
                    .WithMany(p => p.TbCategories)
                    .HasForeignKey(d => d.CashModeCode)
                    .HasConstraintName("FK_Cash_tbCategory_Cash_tbMode");

                entity.HasOne(d => d.CashTypeCodeNavigation)
                    .WithMany(p => p.TbCategories)
                    .HasForeignKey(d => d.CashTypeCode)
                    .HasConstraintName("FK_Cash_tbCategory_Cash_tbType");

                entity.HasOne(d => d.CategoryTypeCodeNavigation)
                    .WithMany(p => p.TbCategories)
                    .HasForeignKey(d => d.CategoryTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbCategory_Cash_tbCategoryType");
            });

            modelBuilder.Entity<Cash_tbCategoryExp>(entity =>
            {
                entity.HasKey(e => e.CategoryCode)
                    .HasName("PK_Cash_tbCategoryExp");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CategoryCodeNavigation)
                    .WithOne(p => p.TbCategoryExp)
                    .HasForeignKey<Cash_tbCategoryExp>(d => d.CategoryCode)
                    .HasConstraintName("FK_Cash_tbCategoryExp_Cash_tbCategory");
            });

            modelBuilder.Entity<Cash_tbCategoryTotal>(entity =>
            {
                entity.HasKey(e => new { e.ParentCode, e.ChildCode })
                    .HasName("PK_Cash_tbCategoryTotal");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ChildCodeNavigation)
                    .WithMany(p => p.TbCategoryTotalChildCodeNavigations)
                    .HasForeignKey(d => d.ChildCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbCategoryTotal_Cash_tbCategory_Child");

                entity.HasOne(d => d.ParentCodeNavigation)
                    .WithMany(p => p.TbCategoryTotalParentCodeNavigations)
                    .HasForeignKey(d => d.ParentCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbCategoryTotal_Cash_tbCategory_Parent");
            });

            modelBuilder.Entity<Cash_tbCategoryType>(entity =>
            {
                entity.HasKey(e => e.CategoryTypeCode)
                    .HasName("PK_Cash_tbCategoryType");

                entity.Property(e => e.CategoryTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cash_tbChange>(entity =>
            {
                entity.HasKey(e => e.PaymentAddress)
                    .HasName("PK_Cash_tbChange");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ChangeTypeCodeNavigation)
                    .WithMany(p => p.TbChanges)
                    .HasForeignKey(d => d.ChangeTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Cash_tbChange_Cash_tbChangeType");
            });

            modelBuilder.Entity<Invoice_tbChangeLog>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceNumber, e.LogId })
                    .HasName("PK_Invoice_tbChangeLog");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate()))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithMany(p => p.TbChangeLogs)
                    .HasForeignKey(d => d.InvoiceNumber)
                    .HasConstraintName("FK_Invoice_tbChangeLog_tbInvoice");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbInvoiceChangeLogs)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbChangeLog_TrasmitStatusCode");
            });

            modelBuilder.Entity<Task_tbChangeLog>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.LogId })
                    .HasName("PK_Task_tbChangeLog");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate()))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbTaskChangeLogs)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbChangeLog_TrasmitStatusCode");
            });

            modelBuilder.Entity<Cash_tbChangeReference>(entity =>
            {
                entity.HasKey(e => e.PaymentAddress)
                    .HasName("PK_Cash_tbChangeReference");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithOne(p => p.TbChangeReference)
                    .HasForeignKey<Cash_tbChangeReference>(d => d.InvoiceNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbChangeReferencee_Invoice_tbInvoice");

                entity.HasOne(d => d.PaymentAddressNavigation)
                    .WithOne(p => p.TbChangeReference)
                    .HasForeignKey<Cash_tbChangeReference>(d => d.PaymentAddress)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbChangeReferencee_Cash_tbChange");
            });

            modelBuilder.Entity<Cash_tbChangeStatus>(entity =>
            {
                entity.HasKey(e => e.ChangeStatusCode)
                    .HasName("PK_Cash_tbChangeStatus");

                entity.Property(e => e.ChangeStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cash_tbChangeType>(entity =>
            {
                entity.HasKey(e => e.ChangeTypeCode)
                    .HasName("PK_Cash_tbChangeType");

                entity.Property(e => e.ChangeTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cash_tbCode>(entity =>
            {
                entity.HasKey(e => e.CashCode)
                    .HasName("PK_Cash_tbCode");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsEnabled).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CategoryCodeNavigation)
                    .WithMany(p => p.TbCodes)
                    .HasForeignKey(d => d.CategoryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbCode_Cash_tbCategory1");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbCodes)
                    .HasForeignKey(d => d.TaxCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbCode_App_tbTaxCode");
            });

            modelBuilder.Entity<App_tbCodeExclusion>(entity =>
            {
                entity.HasKey(e => e.ExcludedTag)
                    .HasName("PK_App_tbCodeExclusion");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Cash_tbCoinType>(entity =>
            {
                entity.HasKey(e => e.CoinTypeCode)
                    .HasName("PK_Cash_tbCoinType");

                entity.Property(e => e.CoinTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Org_tbContact>(entity =>
            {
                entity.HasKey(e => new { e.AccountCode, e.ContactName })
                    .HasName("PK_Org_tbContact")
                    .IsClustered(false);

                entity.HasIndex(e => e.Department, "IX_Org_tbContactDepartment")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.JobTitle, "IX_Org_tbContactJobTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.NameTitle, "IX_Org_tbContactNameTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.AccountCode, "IX_Org_tbContact_AccountCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.OnMailingList).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbContacts)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Org_tbContact_AccountCode");
            });

            modelBuilder.Entity<Task_tbCostSet>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.UserId })
                    .HasName("PK_Task_tbCostSet");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbCostSets)
                    .HasForeignKey(d => d.TaskCode)
                    .HasConstraintName("FK_Task_tbCostSet_Task_tbTask");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbCostSets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Task_tbCostSet_Usr_tbUser");
            });

            modelBuilder.Entity<App_tbDoc>(entity =>
            {
                entity.HasKey(e => new { e.DocTypeCode, e.ReportName })
                    .HasName("PK_App_tbDoc");

                entity.Property(e => e.OpenMode).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.OpenModeNavigation)
                    .WithMany(p => p.TbDocs)
                    .HasForeignKey(d => d.OpenMode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbDoc_Usr_tbMenuOpenMode");
            });

            modelBuilder.Entity<Org_tbDoc>(entity =>
            {
                entity.HasKey(e => new { e.AccountCode, e.DocumentName })
                    .HasName("PK_Org_tbDoc")
                    .IsClustered(false);

                entity.HasIndex(e => e.AccountCode, "IX_Org_tbDoc_AccountCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.DocumentName, e.AccountCode }, "IX_Org_tbDoc_DocName_AccountCode")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbDocs)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Org_tbDoc_AccountCode");
            });

            modelBuilder.Entity<Task_tbDoc>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.DocumentName })
                    .HasName("PK_Task_tbDoc");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbDocs)
                    .HasForeignKey(d => d.TaskCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbDoc_Task_tb");
            });

            modelBuilder.Entity<App_tbDocClass>(entity =>
            {
                entity.HasKey(e => e.DocClassCode)
                    .HasName("PK_App_tbDocClass");

                entity.Property(e => e.DocClassCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbDocSpool>(entity =>
            {
                entity.HasKey(e => new { e.UserName, e.DocTypeCode, e.DocumentNumber })
                    .HasName("PK_App_tbDocSpool");

                entity.HasIndex(e => e.DocTypeCode, "IX_App_tbDocSpool_DocTypeCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.UserName).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.DocTypeCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.SpooledOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.DocTypeCodeNavigation)
                    .WithMany(p => p.TbDocSpools)
                    .HasForeignKey(d => d.DocTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbDocSpool_App_tbDocType");
            });

            modelBuilder.Entity<App_tbDocType>(entity =>
            {
                entity.HasKey(e => e.DocTypeCode)
                    .HasName("PK_App_tbDocType");

                entity.Property(e => e.DocTypeCode).ValueGeneratedNever();

                entity.HasOne(d => d.DocClassCodeNavigation)
                    .WithMany(p => p.TbDocTypes)
                    .HasForeignKey(d => d.DocClassCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbDocType_App_tbDocClass");
            });

            modelBuilder.Entity<Invoice_tbEntry>(entity =>
            {
                entity.HasKey(e => new { e.AccountCode, e.CashCode })
                    .HasName("PK_Invoice_tbEntry");

                entity.Property(e => e.InvoicedOn).HasDefaultValueSql("(CONVERT([date],getdate()))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbEntry_Org_tb");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.CashCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbEntry_Cash_tbCode");

                entity.HasOne(d => d.InvoiceTypeCodeNavigation)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.InvoiceTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbEntry_Invoice_tbType");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Invoice_tbEntry_App_tbTaxCode");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbEntry_Usr_tb");
            });

            modelBuilder.Entity<Cash_tbEntryType>(entity =>
            {
                entity.HasKey(e => e.CashEntryTypeCode)
                    .HasName("PK_Cash_tbEntryType");

                entity.Property(e => e.CashEntryTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbEth>(entity =>
            {
                entity.HasKey(e => e.NetworkProvider)
                    .HasName("PK_App_tbEth");
            });

            modelBuilder.Entity<App_tbEventLog>(entity =>
            {
                entity.HasKey(e => e.LogCode)
                    .HasName("PK_App_tbEventLog_LogCode");

                entity.Property(e => e.EventTypeCode).HasDefaultValueSql("((2))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.LoggedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.EventTypeCodeNavigation)
                    .WithMany(p => p.TbEventLogs)
                    .HasForeignKey(d => d.EventTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tbEventLo__Event__178D7CA5");
            });

            modelBuilder.Entity<App_tbEventType>(entity =>
            {
                entity.HasKey(e => e.EventTypeCode)
                    .HasName("PK_tbFeedLogEventCode");

                entity.Property(e => e.EventTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbFlow>(entity =>
            {
                entity.HasKey(e => new { e.ParentCode, e.StepNumber })
                    .HasName("PK_Activity_tbFlow")
                    .IsClustered(false);

                entity.HasIndex(e => new { e.ChildCode, e.ParentCode }, "IX_Activity_tbFlow_ChildParent")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ParentCode, e.ChildCode }, "IX_Activity_tbFlow_ParentChild")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.StepNumber).HasDefaultValueSql("((10))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UsedOnQuantity).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.ChildCodeNavigation)
                    .WithMany(p => p.TbFlowChildCodeNavigations)
                    .HasForeignKey(d => d.ChildCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbFlow_Activity_tbChild");

                entity.HasOne(d => d.ParentCodeNavigation)
                    .WithMany(p => p.TbFlowParentCodeNavigations)
                    .HasForeignKey(d => d.ParentCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbFlow_tbActivityParent");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbActivityFlows)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbFlow_Activity_tbSyncType");
            });

            modelBuilder.Entity<Task_tbFlow>(entity =>
            {
                entity.HasKey(e => new { e.ParentTaskCode, e.StepNumber })
                    .HasName("PK_Task_tbFlow");

                entity.HasIndex(e => new { e.ChildTaskCode, e.ParentTaskCode }, "IX_Task_tbFlow_ChildParent")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ParentTaskCode, e.ChildTaskCode }, "IX_Task_tbFlow_ParentChild")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.StepNumber).HasDefaultValueSql("((10))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UsedOnQuantity).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.ChildTaskCodeNavigation)
                    .WithMany(p => p.TbFlowChildTaskCodeNavigations)
                    .HasForeignKey(d => d.ChildTaskCode)
                    .HasConstraintName("FK_Task_tbFlow_Task_tb_Child");

                entity.HasOne(d => d.ParentTaskCodeNavigation)
                    .WithMany(p => p.TbFlowParentTaskCodeNavigations)
                    .HasForeignKey(d => d.ParentTaskCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbFlow_Task_tb_Parent");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbTaskFlows)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbFlow_Activity_tbSyncType");
            });

            modelBuilder.Entity<App_tbInstall>(entity =>
            {
                entity.HasKey(e => e.InstallId)
                    .HasName("PK_App_tbInstall");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Usr_tbInterface>(entity =>
            {
                entity.HasKey(e => e.InterfaceCode)
                    .HasName("PK_Usr_tbInterface");

                entity.Property(e => e.InterfaceCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Invoice_tbInvoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceNumber)
                    .HasName("PK_Invoice_tbInvoicePK");

                entity.HasIndex(e => new { e.AccountCode, e.InvoicedOn }, "IX_Invoice_tb_AccountCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.InvoiceStatusCode, e.InvoicedOn }, "IX_Invoice_tb_Status")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.UserId, e.InvoiceNumber }, "IX_Invoice_tb_UserId")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.DueOn).HasDefaultValueSql("(dateadd(day,(1),CONVERT([date],getdate())))");

                entity.Property(e => e.ExpectedOn).HasDefaultValueSql("(dateadd(day,(1),CONVERT([date],getdate())))");

                entity.Property(e => e.InvoicedOn).HasDefaultValueSql("(CONVERT([date],getdate()))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbInvoices)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tb_Org_tb");

                entity.HasOne(d => d.InvoiceStatusCodeNavigation)
                    .WithMany(p => p.TbInvoices)
                    .HasForeignKey(d => d.InvoiceStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tb_Invoice_tbStatus");

                entity.HasOne(d => d.InvoiceTypeCodeNavigation)
                    .WithMany(p => p.TbInvoices)
                    .HasForeignKey(d => d.InvoiceTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tb_Invoice_tbType");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbInvoices)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tb_Usr_tb");
            });

            modelBuilder.Entity<Invoice_tbItem>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceNumber, e.CashCode })
                    .HasName("PK_Invoice_tbItem");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbItems)
                    .HasForeignKey(d => d.CashCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbItem_Cash_tbCode");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithMany(p => p.TbItems)
                    .HasForeignKey(d => d.InvoiceNumber)
                    .HasConstraintName("FK_Invoice_tbItem_Invoice_tb");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbItems)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Invoice_tbItem_App_tbTaxCode");
            });

            modelBuilder.Entity<Usr_tbMenu>(entity =>
            {
                entity.HasKey(e => e.MenuId)
                    .HasName("PK_Usr_tbMenu");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.InterfaceCodeNavigation)
                    .WithMany(p => p.TbMenus)
                    .HasForeignKey(d => d.InterfaceCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usr_tbMenu_Usr_tbInterface");
            });

            modelBuilder.Entity<Usr_tbMenuCommand>(entity =>
            {
                entity.HasKey(e => e.Command)
                    .HasName("PK_Usr_tbMenuCommand");

                entity.Property(e => e.Command).ValueGeneratedNever();
            });

            modelBuilder.Entity<Usr_tbMenuEntry>(entity =>
            {
                entity.HasKey(e => new { e.MenuId, e.EntryId })
                    .HasName("PK_Usr_tbMenuEntry");

                entity.HasIndex(e => e.Command, "IX_Usr_tbMenuEntry_Command")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.OpenMode, "IX_Usr_tbMenuEntry_OpenMode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.EntryId).ValueGeneratedOnAdd();

                entity.Property(e => e.Command).HasDefaultValueSql("((0))");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OpenMode).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CommandNavigation)
                    .WithMany(p => p.TbMenuEntries)
                    .HasForeignKey(d => d.Command)
                    .HasConstraintName("Usr_tbMenuEntry_FK01");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.TbMenuEntries)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_Usr_tbMenuEntry_Usr_tbMenu");

                entity.HasOne(d => d.OpenModeNavigation)
                    .WithMany(p => p.TbMenuEntries)
                    .HasForeignKey(d => d.OpenMode)
                    .HasConstraintName("Usr_tbMenuEntry_FK02");
            });

            modelBuilder.Entity<Usr_tbMenuOpenMode>(entity =>
            {
                entity.HasKey(e => e.OpenMode)
                    .HasName("PK_Usr_tbMenuOpenMode");

                entity.Property(e => e.OpenMode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Usr_tbMenuUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.MenuId })
                    .HasName("PK_Usr_tbMenuUser");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.TbMenuUsers)
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usr_tbMenu_Usr_tbMenu");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbMenuUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usr_tbMenu_Usr_tb");
            });

            modelBuilder.Entity<Usr_tbMenuView>(entity =>
            {
                entity.Property(e => e.MenuViewCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbMirror>(entity =>
            {
                entity.HasKey(e => new { e.ActivityCode, e.AccountCode, e.AllocationCode })
                    .HasName("PK_Activity_tbMirror");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Activity_tbMirror_tbOrg");

                entity.HasOne(d => d.ActivityCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.ActivityCode)
                    .HasConstraintName("FK_Activity_tbMirror_tbActivity");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbActivityMirrors)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbMirror_tbTransmitStatus");
            });

            modelBuilder.Entity<Cash_tbMirror>(entity =>
            {
                entity.HasKey(e => new { e.CashCode, e.AccountCode, e.ChargeCode })
                    .HasName("PK_Cash_tbMirror");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbCashMirror)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbMirror_tbOrg");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.CashCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbMirror_tbCode");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbCashMirrors)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbMirror_tbTransmitStatus");
            });

            modelBuilder.Entity<Invoice_tbMirror>(entity =>
            {
                entity.HasKey(e => e.ContractAddress)
                    .HasName("PK_Invoice_tbMirror");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbInvoiceMirror)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbMirror_tbOrg");

                entity.HasOne(d => d.InvoiceStatusCodeNavigation)
                    .WithMany(p => p.TbMirror)
                    .HasForeignKey(d => d.InvoiceStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbMirror_tbStatus");

                entity.HasOne(d => d.InvoiceTypeCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.InvoiceTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbMirror_tbType");
            });

            modelBuilder.Entity<Invoice_tbMirrorEvent>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.LogId })
                    .HasName("PK_Invoice_tbMirrorEvent");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbMirrorEvents)
                    .HasForeignKey(d => d.ContractAddress)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbMirrorEvent_ContractAddress");

                entity.HasOne(d => d.EventTypeCodeNavigation)
                    .WithMany(p => p.TbMirrorEvents)
                    .HasForeignKey(d => d.EventTypeCode)
                    .HasConstraintName("FK_Invoice_tbMirrorEvent_EventTypeCode");
            });

            modelBuilder.Entity<Invoice_tbMirrorItem>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.ChargeCode })
                    .HasName("PK_Invoice_tbMirrorItem");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbMirrorItems)
                    .HasForeignKey(d => d.ContractAddress)
                    .HasConstraintName("FK_Invoice_tbMirrorItem_ContractAddress");
            });

            modelBuilder.Entity<Invoice_tbMirrorReference>(entity =>
            {
                entity.HasKey(e => e.ContractAddress)
                    .HasName("PK_Invoice_tbMirrorReference");

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithOne(p => p.TbMirrorReference)
                    .HasForeignKey<Invoice_tbMirrorReference>(d => d.ContractAddress)
                    .HasConstraintName("FK_Invoice_tbMirrorReference_tbMirror");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithOne(p => p.TbMirrorReference)
                    .HasForeignKey<Invoice_tbMirrorReference>(d => d.InvoiceNumber)
                    .HasConstraintName("FK_Invoice_tbMirrorReference_tbInvoice");
            });

            modelBuilder.Entity<Invoice_tbMirrorTask>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.TaskCode })
                    .HasName("PK_Invoice_tbMirrorTask");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbMirrorTasks)
                    .HasForeignKey(d => d.ContractAddress)
                    .HasConstraintName("FK_Invoice_tbMirrorTask_ContractAddress");
            });

            modelBuilder.Entity<Cash_tbMode>(entity =>
            {
                entity.HasKey(e => e.CashModeCode)
                    .HasName("PK_Cash_tbMode");

                entity.Property(e => e.CashModeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbMonth>(entity =>
            {
                entity.HasKey(e => e.MonthNumber)
                    .HasName("PK_App_tbMonth");

                entity.Property(e => e.MonthNumber).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbOp>(entity =>
            {
                entity.HasKey(e => new { e.ActivityCode, e.OperationNumber })
                    .HasName("PK_Activity_tbOp");

                entity.HasIndex(e => e.Operation, "IX_Activity_tbOp_Operation")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.Duration).HasDefaultValueSql("((0))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.SyncTypeCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ActivityCodeNavigation)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.ActivityCode)
                    .HasConstraintName("FK_Activity_tbOp_tbActivity");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbActivityOps)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activity_tbOp_Activity_tbSyncType");
            });

            modelBuilder.Entity<Task_tbOp>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.OperationNumber })
                    .HasName("PK_Task_tbOp");

                entity.HasIndex(e => new { e.OpStatusCode, e.StartOn }, "IX_Task_tbOp_OpStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.UserId, e.OpStatusCode, e.StartOn }, "IX_Task_tbOp_UserIdOpStatus")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.Duration).HasDefaultValueSql("((0))");

                entity.Property(e => e.EndOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.StartOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.OpStatusCodeNavigation)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.OpStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbOp_Task_tbOpStatus");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbTaskOps)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbOp_Activity_tbSyncType");

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.TaskCode)
                    .HasConstraintName("FK_Task_tbOp_Task_tb");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tbOp_Usr_tb");
            });

            modelBuilder.Entity<Task_tbOpStatus>(entity =>
            {
                entity.HasKey(e => e.OpStatusCode)
                    .HasName("PK_Task_tbOpStatus");

                entity.Property(e => e.OpStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbOption>(entity =>
            {
                entity.HasKey(e => e.Identifier)
                    .HasName("PK_App_tbOptions");

                entity.Property(e => e.BucketIntervalCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.BucketTypeCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.CoinTypeCode).HasDefaultValueSql("((2))");

                entity.Property(e => e.DefaultPrintMode).HasDefaultValueSql("((2))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.TaxHorizon).HasDefaultValueSql("((90))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbOptionAccountCodeNavigations)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_Org_tb");

                entity.HasOne(d => d.BucketIntervalCodeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.BucketIntervalCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_App_tbBucketInterval");

                entity.HasOne(d => d.BucketTypeCodeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.BucketTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_App_tbBucketType");

                entity.HasOne(d => d.CoinTypeCodeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.CoinTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_Cash_tbCoinType");

                entity.HasOne(d => d.MinerAccountCodeNavigation)
                    .WithMany(p => p.TbOptionMinerAccountCodeNavigations)
                    .HasForeignKey(d => d.MinerAccountCode)
                    .HasConstraintName("FK_App_tbOptions_Org_tbOrg");

                entity.HasOne(d => d.MinerFeeCodeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.MinerFeeCode)
                    .HasConstraintName("FK_App_tbOptions_Cash_tbCode");

                entity.HasOne(d => d.NetProfitCodeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.NetProfitCode)
                    .HasConstraintName("FK_App_tbOption_Cash_tbCategory");

                entity.HasOne(d => d.RegisterNameNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.RegisterName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_App_tbRegister");

                entity.HasOne(d => d.UnitOfChargeNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.UnitOfCharge)
                    .HasConstraintName("FK_App_tbUoc_UnitOfCharge");
            });

            modelBuilder.Entity<Org_tbOrg>(entity =>
            {
                entity.HasKey(e => e.AccountCode)
                    .HasName("PK_Org_tbOrg")
                    .IsClustered(false);

                entity.HasIndex(e => e.AccountName, "IX_Org_tb_AccountName")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.AccountSource, "IX_Org_tb_AccountSource")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.AreaCode, "IX_Org_tb_AreaCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.OrganisationStatusCode, "IX_Org_tb_OrganisationStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.OrganisationTypeCode, "IX_Org_tb_OrganisationTypeCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.PaymentTerms, "IX_Org_tb_PaymentTerms")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.OrganisationStatusCode, e.AccountName }, "IX_Org_tb_Status_AccountCode")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrganisationStatusCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.OrganisationTypeCode).HasDefaultValueSql("((1))");

                //entity.Property(e => e.PayBalance).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AddressCodeNavigation)
                    .WithMany(p => p.TbOrgs)
                    .HasForeignKey(d => d.AddressCode)
                    .HasConstraintName("FK_Org_tb_Org_tbAddress");

                entity.HasOne(d => d.OrganisationStatusCodeNavigation)
                    .WithMany(p => p.TbOrgs)
                    .HasForeignKey(d => d.OrganisationStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tbOrg_FK00");

                entity.HasOne(d => d.OrganisationTypeCodeNavigation)
                    .WithMany(p => p.TbOrgs)
                    .HasForeignKey(d => d.OrganisationTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tbOrg_FK01");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbOrgs)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Org_tb_App_tbTaxCode");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbOrgs)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Org_tbOrg_tbTransmitStatus");
            });

            modelBuilder.Entity<Cash_tbPayment>(entity =>
            {
                entity.HasKey(e => e.PaymentCode)
                    .HasName("PK_Cash_tbPayment");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.IsProfitAndLoss).HasDefaultValueSql("((1))");

                entity.Property(e => e.PaidOn).HasDefaultValueSql("(CONVERT([date],getdate()))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_tbOrg");

                entity.HasOne(d => d.CashAccountCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.CashAccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_Org_tbAccount");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Cash_tbPayment_Cash_tbCode");

                entity.HasOne(d => d.PaymentStatusCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.PaymentStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_Cash_tbPaymentStatus");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Cash_tbPayment_App_tbTaxCode");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_Usr_tbUser");
            });

            modelBuilder.Entity<Cash_tbPaymentStatus>(entity =>
            {
                entity.HasKey(e => e.PaymentStatusCode)
                    .HasName("PK_Cash_tbPaymentStatus");

                entity.Property(e => e.PaymentStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbPeriod>(entity =>
            {
                entity.HasKey(e => new { e.CashCode, e.StartOn })
                    .HasName("PK_Cash_tbPeriod");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbPeriods)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Cash_tbPeriod_Cash_tbCode");

                entity.HasOne(d => d.StartOnNavigation)
                    .WithMany(p => p.TbPeriods)
                    .HasPrincipalKey(p => p.StartOn)
                    .HasForeignKey(d => d.StartOn)
                    .HasConstraintName("FK_Cash_tbPeriod_App_tbYearPeriod");
            });

            modelBuilder.Entity<Task_tbQuote>(entity =>
            {
                entity.HasKey(e => new { e.TaskCode, e.Quantity })
                    .HasName("PK_Task_tbQuote");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbQuotes)
                    .HasForeignKey(d => d.TaskCode)
                    .HasConstraintName("FK_Task_tbQuote_Task_tb");
            });

            modelBuilder.Entity<App_tbRecurrence>(entity =>
            {
                entity.HasKey(e => e.RecurrenceCode)
                    .HasName("PK_App_tbRecurrence");

                entity.Property(e => e.RecurrenceCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbRegister>(entity =>
            {
                entity.HasKey(e => e.RegisterName)
                    .HasName("PK_App_tbRegister");

                entity.Property(e => e.NextNumber).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_tbRounding>(entity =>
            {
                entity.Property(e => e.RoundingCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Org_tbSector>(entity =>
            {
                entity.HasKey(e => new { e.AccountCode, e.IndustrySector })
                    .HasName("PK_Org_tbSector");

                entity.HasIndex(e => e.IndustrySector, "IX_Org_tbSector_IndustrySector")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbSectors)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Org_tbSector_Org_tb");
            });

            modelBuilder.Entity<Cash_tbStatus>(entity =>
            {
                entity.HasKey(e => e.CashStatusCode)
                    .HasName("PK_Cash_tbStatus");

                entity.Property(e => e.CashStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Invoice_tbStatus>(entity =>
            {
                entity.HasKey(e => e.InvoiceStatusCode)
                    .HasName("PK_Invoice_tbStatus")
                    .IsClustered(false);

                entity.Property(e => e.InvoiceStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Org_tbStatus>(entity =>
            {
                entity.HasKey(e => e.OrganisationStatusCode)
                    .HasName("PK_Org_tbStatus")
                    .IsClustered(false);

                entity.Property(e => e.OrganisationStatusCode).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Task_tbStatus>(entity =>
            {
                entity.HasKey(e => e.TaskStatusCode)
                    .HasName("PK_Task_tbStatus")
                    .IsClustered(false);

                entity.HasIndex(e => e.TaskStatus, "IX_Task_tbStatus_TaskStatus")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.TaskStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Activity_tbSyncType>(entity =>
            {
                entity.HasKey(e => e.SyncTypeCode)
                    .HasName("PK_Activity_tbSyncType");

                entity.Property(e => e.SyncTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Invoice_tbTask>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceNumber, e.TaskCode })
                    .HasName("PK_Invoice_tbTask");

                entity.HasIndex(e => new { e.CashCode, e.InvoiceNumber }, "IX_Invoice_tbTask_CashCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbInvoiceTasks)
                    .HasForeignKey(d => d.CashCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbTask_Cash_tbCode");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.InvoiceNumber)
                    .HasConstraintName("FK_Invoice_tbTask_Invoice_tb");

                entity.HasOne(d => d.TaskCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.TaskCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbTask_Task_tb");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbInvoiceTasks)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Invoice_tbTask_App_tbTaxCode");
            });

            modelBuilder.Entity<Task_tbTask>(entity =>
            {
                entity.HasKey(e => e.TaskCode)
                    .HasName("PK_Task_tbTask");

                entity.HasIndex(e => e.AccountCode, "IX_Task_tb_AccountCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.AccountCode, e.ActionOn }, "IX_Task_tb_AccountCodeByActionOn")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.AccountCode, e.TaskStatusCode, e.ActionOn }, "IX_Task_tb_AccountCodeByStatus")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ActionById, e.TaskStatusCode, e.ActionOn }, "IX_Task_tb_ActionBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ActionById, "IX_Task_tb_ActionById")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ActionOn, "IX_Task_tb_ActionOn")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.TaskStatusCode, e.ActionOn, e.AccountCode }, "IX_Task_tb_ActionOnStatus")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ActivityCode, "IX_Task_tb_ActivityCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ActivityCode, e.TaskTitle }, "IX_Task_tb_ActivityCodeTaskTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.TaskStatusCode, e.ActionOn, e.AccountCode }, "IX_Task_tb_ActivityStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.CashCode, e.TaskStatusCode, e.ActionOn }, "IX_Task_tb_CashCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.TaskStatusCode, "IX_Task_tb_TaskStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.UserId, "IX_Task_tb_UserId")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.ActionOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PaymentOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Activity_tb_FK02");

                entity.HasOne(d => d.ActionBy)
                    .WithMany(p => p.TbTaskActionBys)
                    .HasForeignKey(d => d.ActionById)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tb_Usr_tb_ActionById");

                entity.HasOne(d => d.ActivityCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.ActivityCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Activity_tb_FK00");

                entity.HasOne(d => d.AddressCodeFromNavigation)
                    .WithMany(p => p.TbTaskAddressCodeFromNavigations)
                    .HasForeignKey(d => d.AddressCodeFrom)
                    .HasConstraintName("FK_Task_tb_Org_tbAddress_From");

                entity.HasOne(d => d.AddressCodeToNavigation)
                    .WithMany(p => p.TbTaskAddressCodeToNavigations)
                    .HasForeignKey(d => d.AddressCodeTo)
                    .HasConstraintName("FK_Task_tb_Org_tbAddress_To");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Task_tb_Cash_tbCode");

                entity.HasOne(d => d.TaskStatusCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.TaskStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Activity_tb_FK01");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbTasks)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Task_tb_App_tbTaxCode");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbTaskUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_tb_Usr_tb");
            });

            modelBuilder.Entity<App_tbTaxCode>(entity =>
            {
                entity.HasKey(e => e.TaxCode)
                    .HasName("PK_App_tbTaxCode");

                entity.HasIndex(e => new { e.TaxTypeCode, e.TaxCode }, "IX_App_tbTaxCodeByType")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.Decimals).HasDefaultValueSql("((2))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.TaxTypeCode).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.RoundingCodeNavigation)
                    .WithMany(p => p.TbTaxCodes)
                    .HasForeignKey(d => d.RoundingCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbTaxCode_App_tbRounding");

                entity.HasOne(d => d.TaxTypeCodeNavigation)
                    .WithMany(p => p.TbTaxCodes)
                    .HasForeignKey(d => d.TaxTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbTaxCode_Cash_tbTaxType");
            });

            modelBuilder.Entity<Cash_tbTaxType>(entity =>
            {
                entity.HasKey(e => e.TaxTypeCode)
                    .HasName("PK_Cash_tbTaxType");

                entity.Property(e => e.TaxTypeCode).ValueGeneratedNever();

                entity.Property(e => e.MonthNumber).HasDefaultValueSql("((1))");

                entity.Property(e => e.RecurrenceCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbTaxTypes)
                    .HasForeignKey(d => d.AccountCode)
                    .HasConstraintName("FK_Cash_tbTaxType_Org_tb");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbTaxTypes)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Cash_tbTaxType_Cash_tbCode");

                entity.HasOne(d => d.MonthNumberNavigation)
                    .WithMany(p => p.TbTaxTypes)
                    .HasForeignKey(d => d.MonthNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTaxType_App_tbMonth");

                entity.HasOne(d => d.RecurrenceCodeNavigation)
                    .WithMany(p => p.TbTaxTypes)
                    .HasForeignKey(d => d.RecurrenceCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTaxType_App_tbRecurrence");
            });

            modelBuilder.Entity<App_tbText>(entity =>
            {
                entity.HasKey(e => e.TextId)
                    .HasName("PK_App_tbText");

                entity.Property(e => e.TextId).ValueGeneratedNever();

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Org_tbTransmitStatus>(entity =>
            {
                entity.HasKey(e => e.TransmitStatusCode)
                    .HasName("PK_App_tbTransmitStatus");

                entity.Property(e => e.TransmitStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cash_tbTx>(entity =>
            {
                entity.HasKey(e => e.TxNumber)
                    .HasName("PK_Cash_tbTx");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.TransactedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PaymentAddressNavigation)
                    .WithMany(p => p.TbTxes)
                    .HasForeignKey(d => d.PaymentAddress)
                    .HasConstraintName("FK_Cash_tbTx_Cash_tbChange");

                entity.HasOne(d => d.TxStatusCodeNavigation)
                    .WithMany(p => p.TbTxes)
                    .HasForeignKey(d => d.TxStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTx_Cash_tbTxStatus");
            });

            modelBuilder.Entity<Cash_tbTxReference>(entity =>
            {
                entity.HasKey(e => new { e.TxNumber, e.TxStatusCode })
                    .HasName("PK_Cash_tbTxReference");

                entity.HasOne(d => d.PaymentCodeNavigation)
                    .WithMany(p => p.TbTxReferences)
                    .HasForeignKey(d => d.PaymentCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTxReference_Cash_tbPayment");

                entity.HasOne(d => d.TxNumberNavigation)
                    .WithMany(p => p.TbTxReferences)
                    .HasForeignKey(d => d.TxNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTxReference_Cash_tbTx");

                entity.HasOne(d => d.TxStatusCodeNavigation)
                    .WithMany(p => p.TbTxReferences)
                    .HasForeignKey(d => d.TxStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbTxReference_Cash_tbTxStatus");
            });

            modelBuilder.Entity<Cash_tbTxStatus>(entity =>
            {
                entity.HasKey(e => e.TxStatusCode)
                    .HasName("PK_Cash_tbTxStatus");

                entity.Property(e => e.TxStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cash_tbType>(entity =>
            {
                entity.HasKey(e => e.CashTypeCode)
                    .HasName("PK_Cash_tbType");

                entity.Property(e => e.CashTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Invoice_tbType>(entity =>
            {
                entity.HasKey(e => e.InvoiceTypeCode)
                    .HasName("PK_Invoice_tbType");

                entity.Property(e => e.InvoiceTypeCode).ValueGeneratedNever();

                entity.Property(e => e.NextNumber).HasDefaultValueSql("((1000))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashModeCodeNavigation)
                    .WithMany(p => p.TbInvoiceType)
                    .HasForeignKey(d => d.CashModeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbType_Cash_tbMode");
            });

            modelBuilder.Entity<Org_tbType>(entity =>
            {
                entity.HasKey(e => e.OrganisationTypeCode)
                    .HasName("PK_Org_tbType")
                    .IsClustered(false);

                entity.Property(e => e.OrganisationTypeCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashModeCodeNavigation)
                    .WithMany(p => p.TbOrgType)
                    .HasForeignKey(d => d.CashModeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Org_tbType_Cash_tbMode");
            });

            modelBuilder.Entity<App_tbUoc>(entity =>
            {
                entity.HasKey(e => e.UnitOfCharge)
                    .HasName("PK_tbTag");
            });

            modelBuilder.Entity<App_tbUom>(entity =>
            {
                entity.HasKey(e => e.UnitOfMeasure)
                    .HasName("PK_App_tbUom");

            });

            modelBuilder.Entity<Usr_tbUser>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_Usr_tbUser");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsEnabled).HasDefaultValueSql("((1))");

                entity.Property(e => e.LogonName).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.NextTaskNumber).HasDefaultValueSql("((1))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CalendarCodeNavigation)
                    .WithMany(p => p.TbUsers)
                    .HasForeignKey(d => d.CalendarCode)
                    .HasConstraintName("FK_Usr_tb_App_tbCalendar");

                entity.HasOne(d => d.MenuViewCodeNavigation)
                    .WithMany(p => p.TbUsers)
                    .HasForeignKey(d => d.MenuViewCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usr_tbMenu_Usr_tbUser");
            });

            modelBuilder.Entity<App_tbYear>(entity =>
            {
                entity.HasKey(e => e.YearNumber)
                    .HasName("PK_App_tbYear");

                entity.Property(e => e.YearNumber).ValueGeneratedNever();

                entity.Property(e => e.CashStatusCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.StartMonth).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.StartMonthNavigation)
                    .WithMany(p => p.TbYears)
                    .HasForeignKey(d => d.StartMonth)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbYear_App_tbMonth");
            });

            modelBuilder.Entity<App_tbYearPeriod>(entity =>
            {
                entity.HasKey(e => new { e.YearNumber, e.StartOn })
                    .HasName("PK_App_tbYearPeriod");

                entity.Property(e => e.CashStatusCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashStatusCodeNavigation)
                    .WithMany(p => p.TbYearPeriods)
                    .HasForeignKey(d => d.CashStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbYearPeriod_Cash_tbStatus");

                entity.HasOne(d => d.MonthNumberNavigation)
                    .WithMany(p => p.TbYearPeriods)
                    .HasForeignKey(d => d.MonthNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbYearPeriod_App_tbMonth");

                entity.HasOne(d => d.YearNumberNavigation)
                    .WithMany(p => p.TbYearPeriods)
                    .HasForeignKey(d => d.YearNumber)
                    .HasConstraintName("FK_App_tbYearPeriod_App_tbYear");
            });

            modelBuilder.Entity<Org_vwAccountLookup>(entity =>
            {
                entity.HasKey(e => new { e.AccountCode });
                entity.ToView("vwAccountLookup", "Org");
            });

            modelBuilder.Entity<Org_vwAccountSource>(entity =>
            {
                entity.ToView("vwAccountSources", "Org");
            });

            modelBuilder.Entity<Cash_vwAccountStatement>(entity =>
            {
                entity.ToView("vwAccountStatement", "Cash");
            });

            modelBuilder.Entity<Cash_vwAccountStatementListing>(entity =>
            {
                entity.ToView("vwAccountStatementListing", "Cash");
            });

            modelBuilder.Entity<Invoice_vwAccountsMode>(entity =>
            {
                entity.ToView("vwAccountsMode", "Invoice");

                entity.Property(e => e.InvoiceRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.ItemRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Task_vwActiveDatum>(entity =>
            {
                entity.ToView("vwActiveData", "Task");
            });

            modelBuilder.Entity<App_vwActivePeriod>(entity =>
            {
                entity.ToView("vwActivePeriod", "App");
            });

            modelBuilder.Entity<Task_vwActiveStatusCode>(entity =>
            {
                entity.ToView("vwActiveStatusCodes", "Task");
            });

            modelBuilder.Entity<Invoice_vwAgedDebtPurchase>(entity =>
            {
                entity.ToView("vwAgedDebtPurchases", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwAgedDebtSale>(entity =>
            {
                entity.ToView("vwAgedDebtSales", "Invoice");
            });

            modelBuilder.Entity<Task_vwAllocationSvD>(entity =>
            {
                entity.ToView("vwAllocationSvD", "Task");
            });

            modelBuilder.Entity<Org_vwAreaCode>(entity =>
            {
                entity.ToView("vwAreaCodes", "Org");
            });

            modelBuilder.Entity<Org_vwAssetStatementAudit>(entity =>
            {
                entity.ToView("vwAssetStatementAudit", "Org");
            });

            modelBuilder.Entity<Task_vwAttributeDescription>(entity =>
            {
                entity.ToView("vwAttributeDescriptions", "Task");
            });

            modelBuilder.Entity<Task_vwAttributesForOrder>(entity =>
            {
                entity.ToView("vwAttributesForOrder", "Task");
            });

            modelBuilder.Entity<Task_vwAttributesForQuote>(entity =>
            {
                entity.ToView("vwAttributesForQuote", "Task");
            });

            modelBuilder.Entity<Cash_vwBalanceSheet>(entity =>
            {
                entity.ToView("vwBalanceSheet", "Cash");
            });

            modelBuilder.Entity<Org_vwBalanceSheetAudit>(entity =>
            {
                entity.ToView("vwBalanceSheetAudit", "Org");
            });

            modelBuilder.Entity<Cash_vwBankCashCode>(entity =>
            {
                entity.ToView("vwBankCashCodes", "Cash");
            });

            modelBuilder.Entity<Cash_vwBudget>(entity =>
            {
                entity.ToView("vwBudget", "Cash");
            });

            modelBuilder.Entity<Cash_vwBudgetDataEntry>(entity =>
            {
                entity.ToView("vwBudgetDataEntry", "Cash");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Activity_wCandidateCashCode>(entity =>
            {
                entity.ToView("vwCandidateCashCodes", "Activity");
            });

            modelBuilder.Entity<App_vwCandidateCategoryCode>(entity =>
            {
                entity.ToView("vwCandidateCategoryCodes", "App");
            });

            modelBuilder.Entity<Invoice_vwCandidateCredit>(entity =>
            {
                entity.ToView("vwCandidateCredits", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwCandidateDebit>(entity =>
            {
                entity.ToView("vwCandidateDebits", "Invoice");
            });

            modelBuilder.Entity<App_vwCandidateHomeAccount>(entity =>
            {
                entity.ToView("vwCandidateHomeAccounts", "App");
            });

            modelBuilder.Entity<Invoice_vwCandidatePurchase>(entity =>
            {
                entity.ToView("vwCandidatePurchases", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwCandidateSale>(entity =>
            {
                entity.ToView("vwCandidateSales", "Invoice");
            });

            modelBuilder.Entity<Org_vwCashAccountAsset>(entity =>
            {
                entity.ToView("vwCashAccountAssets", "Org");
            });

            modelBuilder.Entity<Org_vwCashAccount>(entity =>
            {
                entity.ToView("vwCashAccounts", "Org");
            });

            modelBuilder.Entity<Cash_vwCashFlowType>(entity =>
            {
                entity.ToView("vwCashFlowTypes", "Cash");
            });

            modelBuilder.Entity<Cash_vwCategoryBudget>(entity =>
            {
                entity.ToView("vwCategoryBudget", "Cash");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Cash_vwCategoryTotal>(entity =>
            {
                entity.ToView("vwCategoryTotals", "Cash");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Cash_vwCategoryTotalCandidate>(entity =>
            {
                entity.ToView("vwCategoryTotalCandidates", "Cash");
            });

            modelBuilder.Entity<Cash_vwCategoryTrade>(entity =>
            {
                entity.ToView("vwCategoryTrade", "Cash");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Invoice_vwChangeLog>(entity =>
            {
                entity.ToView("vwChangeLog", "Invoice");
            });

            modelBuilder.Entity<Task_vwChangeLog>(entity =>
            {
                entity.ToView("vwChangeLog", "Task");
            });

            modelBuilder.Entity<Activity_vwCode>(entity =>
            {
                entity.ToView("vwCodes", "Activity");
            });

            modelBuilder.Entity<Cash_vwCodeLookup>(entity =>
            {
                entity.HasKey(e => new { e.CashCode });
                entity.ToView("vwCodeLookup", "Cash");
            });

            modelBuilder.Entity<Org_vwCompanyHeader>(entity =>
            {
                entity.ToView("vwCompanyHeader", "Org");
            });

            modelBuilder.Entity<Org_vwCompanyLogo>(entity =>
            {
                entity.ToView("vwCompanyLogo", "Org");
            });

            modelBuilder.Entity<Org_vwContact>(entity =>
            {
                entity.ToView("vwContacts", "Org");
            });

            modelBuilder.Entity<Task_vwCostSet>(entity =>
            {
                entity.ToView("vwCostSet", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Usr_vwCredential>(entity =>
            {
                entity.ToView("vwCredentials", "Usr");
            });

            modelBuilder.Entity<Invoice_vwCreditNoteSpool>(entity =>
            {
                entity.ToView("vwCreditNoteSpool", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwCreditSpoolByItem>(entity =>
            {
                entity.ToView("vwCreditSpoolByItem", "Invoice");
            });


            modelBuilder.Entity<Org_vwDatasheet>(entity =>
            {
                entity.ToView("vwDatasheet", "Org");
            });

            modelBuilder.Entity<Invoice_vwDebitNoteSpool>(entity =>
            {
                entity.ToView("vwDebitNoteSpool", "Invoice");
            });

            modelBuilder.Entity<Activity_vwDefaultText>(entity =>
            {
                entity.ToView("vwDefaultText", "Activity");
            });

            modelBuilder.Entity<Org_vwDepartment>(entity =>
            {
                entity.ToView("vwDepartments", "Org");
            });

            modelBuilder.Entity<App_vwDocCreditNote>(entity =>
            {
                entity.ToView("vwDocCreditNote", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocDebitNote>(entity =>
            {
                entity.ToView("vwDocDebitNote", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocOpenMode>(entity =>
            {
                entity.ToView("vwDocOpenModes", "App");
            });

            modelBuilder.Entity<App_vwDocPurchaseEnquiry>(entity =>
            {
                entity.ToView("vwDocPurchaseEnquiry", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocPurchaseOrder>(entity =>
            {
                entity.ToView("vwDocPurchaseOrder", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocQuotation>(entity =>
            {
                entity.ToView("vwDocQuotation", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocSalesInvoice>(entity =>
            {
                entity.ToView("vwDocSalesInvoice", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwDocSalesOrder>(entity =>
            {
                entity.ToView("vwDocSalesOrder", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwEventLog>(entity =>
            {
                entity.ToView("vwEventLog", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Activity_vwExpenseCashCode>(entity =>
            {
                entity.ToView("vwExpenseCashCodes", "Activity");
            });

            modelBuilder.Entity<Cash_vwExternalCodesLookup>(entity =>
            {
                entity.ToView("vwExternalCodesLookup", "Cash");
            });

            modelBuilder.Entity<Task_vwFlow>(entity =>
            {
                entity.ToView("vwFlow", "Task");
            });

            modelBuilder.Entity<App_vwGraphBankBalance>(entity =>
            {
                entity.ToView("vwGraphBankBalance", "App");
            });

            modelBuilder.Entity<App_vwGraphTaskActivity>(entity =>
            {
                entity.ToView("vwGraphTaskActivity", "App");
            });

            modelBuilder.Entity<Invoice_vwHistoryCashCode>(entity =>
            {
                entity.ToView("vwHistoryCashCodes", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwHistoryPurchase>(entity =>
            {
                entity.ToView("vwHistoryPurchases", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwHistoryPurchaseItem>(entity =>
            {
                entity.ToView("vwHistoryPurchaseItems", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwHistorySale>(entity =>
            {
                entity.ToView("vwHistorySales", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwHistorySalesItem>(entity =>
            {
                entity.ToView("vwHistorySalesItems", "Invoice");
            });

            modelBuilder.Entity<App_vwIdentity>(entity =>
            {
                entity.ToView("vwIdentity", "App");
            });

            modelBuilder.Entity<Activity_wIncomeCashCode>(entity =>
            {
                entity.ToView("vwIncomeCashCodes", "Activity");
            });

            modelBuilder.Entity<Org_vwInvoiceItem>(entity =>
            {
                entity.ToView("vwInvoiceItems", "Org");
            });

            modelBuilder.Entity<Org_vwInvoiceSummary>(entity =>
            {
                entity.ToView("vwInvoiceSummary", "Org");
            });

            modelBuilder.Entity<Org_vwInvoiceTask>(entity =>
            {
                entity.ToView("vwInvoiceTasks", "Org");
            });

            modelBuilder.Entity<Invoice_vwItem>(entity =>
            {
                entity.ToView("vwItems", "Invoice");
            });

            modelBuilder.Entity<Org_vwJobTitle>(entity =>
            {
                entity.ToView("vwJobTitles", "Org");
            });

            modelBuilder.Entity<Org_vwListActive>(entity =>
            {
                entity.ToView("vwListActive", "Org");
            });

            modelBuilder.Entity<Org_vwListAll>(entity =>
            {
                entity.ToView("vwListAll", "Org");
            });

            modelBuilder.Entity<Invoice_vwMirror>(entity =>
            {
                entity.ToView("vwMirrors", "Invoice");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Invoice_vwMirrorDetail>(entity =>
            {
                entity.ToView("vwMirrorDetails", "Invoice");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Invoice_vwMirrorEvent>(entity =>
            {
                entity.ToView("vwMirrorEvents", "Invoice");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });


            modelBuilder.Entity<Task_vwNetworkAllocation>(entity =>
            {
                entity.ToView("vwNetworkAllocations", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Invoice_vwNetworkChangeLog>(entity =>
            {
                entity.ToView("vwNetworkChangeLog", "Invoice");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Task_vwNetworkChangeLog>(entity =>
            {
                entity.ToView("vwNetworkChangeLog", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Task_vwNetworkEvent>(entity =>
            {
                entity.ToView("vwNetworkEvents", "Task");
            });

            modelBuilder.Entity<Task_vwNetworkEventLog>(entity =>
            {
                entity.ToView("vwNetworkEventLog", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Task_vwNetworkQuotation>(entity =>
            {
                entity.ToView("vwNetworkQuotations", "Task");
            });

            modelBuilder.Entity<Task_vwOp>(entity =>
            {
                entity.ToView("vwOps", "Task");

                entity.Property(e => e.OpRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.TaskRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Cash_vwPayment>(entity =>
            {
                entity.ToView("vwPayments", "Cash");
            });

            modelBuilder.Entity<Org_vwPaymentTerm>(entity =>
            {
                entity.ToView("vwPaymentTerms", "Org");
            });

            modelBuilder.Entity<Cash_vwPaymentsListing>(entity =>
            {
                entity.ToView("vwPaymentsListing", "Cash");
            });

            modelBuilder.Entity<Cash_vwPaymentsUnposted>(entity =>
            {
                entity.HasKey(e => e.PaymentCode);
                //entity.ToView("vwPaymentsUnposted", "Cash");

            });

            modelBuilder.Entity<App_vwPeriod>(entity =>
            {
                entity.ToView("vwPeriods", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwPeriodEndListing>(entity =>
            {
                entity.ToView("vwPeriodEndListing", "App");
            });

            modelBuilder.Entity<Task_vwProfit>(entity =>
            {
                entity.ToView("vwProfit", "Task");
            });

            modelBuilder.Entity<Task_vwProfitToDate>(entity =>
            {
                entity.ToView("vwProfitToDate", "Task");
            });

            modelBuilder.Entity<Task_vwPurchase>(entity =>
            {
                entity.ToView("vwPurchases", "Task");
            });

            modelBuilder.Entity<Task_vwPurchaseEnquiryDeliverySpool>(entity =>
            {
                entity.ToView("vwPurchaseEnquiryDeliverySpool", "Task");
            });

            modelBuilder.Entity<Task_vwPurchaseEnquirySpool>(entity =>
            {
                entity.ToView("vwPurchaseEnquirySpool", "Task");
            });

            modelBuilder.Entity<Task_vwPurchaseOrderDeliverySpool>(entity =>
            {
                entity.ToView("vwPurchaseOrderDeliverySpool", "Task");
            });

            modelBuilder.Entity<Task_vwPurchaseOrderSpool>(entity =>
            {
                entity.ToView("vwPurchaseOrderSpool", "Task");
            });

            modelBuilder.Entity<Task_vwQuotationSpool>(entity =>
            {
                entity.ToView("vwQuotationSpool", "Task");
            });

            modelBuilder.Entity<Task_vwQuote>(entity =>
            {
                entity.ToView("vwQuotes", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Invoice_vwRegister>(entity =>
            {
                entity.ToView("vwRegister", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterCashCode>(entity =>
            {
                entity.ToView("vwRegisterCashCodes", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterDetail>(entity =>
            {
                entity.ToView("vwRegisterDetail", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterExpense>(entity =>
            {
                entity.ToView("vwRegisterExpenses", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterItem>(entity =>
            {
                entity.ToView("vwRegisterItems", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterPurchase>(entity =>
            {
                entity.ToView("vwRegisterPurchases", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterPurchaseTask>(entity =>
            {
                entity.ToView("vwRegisterPurchaseTasks", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterPurchasesOverdue>(entity =>
            {
                entity.ToView("vwRegisterPurchasesOverdue", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSale>(entity =>
            {
                entity.ToView("vwRegisterSales", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSaleTask>(entity =>
            {
                entity.ToView("vwRegisterSaleTasks", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSalesOverdue>(entity =>
            {
                entity.ToView("vwRegisterSalesOverdue", "Invoice");
            });

            modelBuilder.Entity<Task_vwSale>(entity =>
            {
                entity.ToView("vwSales", "Task");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpool>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpool", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpoolByActivity>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpoolByActivity", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpoolByItem>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpoolByItem", "Invoice");
            });

            modelBuilder.Entity<Task_vwSalesOrderSpool>(entity =>
            {
                entity.ToView("vwSalesOrderSpool", "Task");
            });

            modelBuilder.Entity<Cash_vwStatement>(entity =>
            {
                entity.ToView("vwStatement", "Cash");
            });

            modelBuilder.Entity<Org_wStatement>(entity =>
            {
                entity.ToView("vwStatement", "Org");
            });

            modelBuilder.Entity<Org_vwStatementReport>(entity =>
            {
                entity.ToView("vwStatementReport", "Org");
            });

            modelBuilder.Entity<Cash_vwStatementReserve>(entity =>
            {
                entity.ToView("vwStatementReserves", "Cash");
            });

            modelBuilder.Entity<Cash_vwStatementWhatIf>(entity =>
            {
                entity.ToView("vwStatementWhatIf", "Cash");
            });

            modelBuilder.Entity<Org_vwStatusReport>(entity =>
            {
                entity.ToView("vwStatusReport", "Org");
            });

            modelBuilder.Entity<Cash_vwSummary>(entity =>
            {
                entity.ToView("vwSummary", "Cash");
            });

            modelBuilder.Entity<Invoice_vwSummary>(entity =>
            {
                entity.ToView("vwSummary", "Invoice");
            });

            modelBuilder.Entity<Org_vwTask>(entity =>
            {
                entity.ToView("vwTasks", "Org");
            });

            modelBuilder.Entity<Task_vwTask>(entity =>
            {
                entity.ToView("vwTasks", "Task");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwTaxCode>(entity =>
            {
                entity.HasKey(e => new { e.TaxCode });
                entity.ToView("vwTaxCodes", "App");
            });

            modelBuilder.Entity<App_vwTaxCodeType>(entity =>
            {
                entity.ToView("vwTaxCodeTypes", "App");
            });

            modelBuilder.Entity<Cash_vwTaxCorpAuditAccrual>(entity =>
            {
                entity.ToView("vwTaxCorpAuditAccruals", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxCorpStatement>(entity =>
            {
                entity.ToView("vwTaxCorpStatement", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxCorpTotal>(entity =>
            {
                entity.ToView("vwTaxCorpTotals", "Cash");
            });

            modelBuilder.Entity<Invoice_vwTaxSummary>(entity =>
            {
                entity.ToView("vwTaxSummary", "Invoice");
            });

            modelBuilder.Entity<Cash_vwTaxVatAuditAccrual>(entity =>
            {
                entity.ToView("vwTaxVatAuditAccruals", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxVatAuditInvoice>(entity =>
            {
                entity.ToView("vwTaxVatAuditInvoices", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxVatDetail>(entity =>
            {
                entity.ToView("vwTaxVatDetails", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxVatStatement>(entity =>
            {
                entity.ToView("vwTaxVatStatement", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxVatSummary>(entity =>
            {
                entity.ToView("vwTaxVatSummary", "Cash");
            });

            modelBuilder.Entity<Cash_vwTaxVatTotal>(entity =>
            {
                entity.ToView("vwTaxVatTotals", "Cash");
            });

            modelBuilder.Entity<Task_vwTitle>(entity =>
            {
                entity.ToView("vwTitles", "Task");
            });

            modelBuilder.Entity<Cash_vwTransferCodeLookup>(entity =>
            {
                entity.ToView("vwTransferCodeLookup", "Cash");                
            });

            modelBuilder.Entity<Cash_vwTransfersUnposted>(entity =>
            {
                entity.HasKey(e => e.PaymentCode);
            });

            modelBuilder.Entity<Org_vwTypeLookup>(entity =>
            {
                entity.ToView("vwTypeLookup", "Org");
            });

            modelBuilder.Entity<Activity_vwUnMirrored>(entity =>
            {
                entity.ToView("vwUnMirrored", "Activity");
            });

            modelBuilder.Entity<Cash_vwUnMirrored>(entity =>
            {
                entity.ToView("vwUnMirrored", "Cash");
            });

            modelBuilder.Entity<Usr_vwUserMenu>(entity =>
            {
                entity.ToView("vwUserMenus", "Usr");
            });

            modelBuilder.Entity<Usr_vwUserMenuList>(entity =>
            {
                entity.ToView("vwUserMenuList", "Usr");
            });

            modelBuilder.Entity<Cash_vwVatcode>(entity =>
            {
                entity.ToView("vwVATCodes", "Cash");
            });

            modelBuilder.Entity<App_vwHomeAccount>(entity =>
            {
                entity.ToView("vwHomeAccount", "App");
            });

            modelBuilder.Entity<App_vwVersion>(entity =>
            {
                entity.ToView("vwVersion", "App");

                entity.Property(e => e.VersionString).IsUnicode(false);
            });

            modelBuilder.Entity<App_vwWarehouseOrg>(entity =>
            {
                entity.ToView("vwWarehouseOrg", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwWarehouseTask>(entity =>
            {
                entity.ToView("vwWarehouseTask", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwYearPeriod>(entity =>
            {
                entity.ToView("vwYearPeriod", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        #endregion


    }
}
