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
        public virtual DbSet<Subject_tbAccount> Subject_tbAccounts { get; set; }
        public virtual DbSet<Subject_tbAccountType> Subject_tbAccountTypes { get; set; }
        public virtual DbSet<Object_tbObject> Object_tbActivities { get; set; }
        public virtual DbSet<Subject_tbAddress> Subject_tbAddresses { get; set; }
        public virtual DbSet<Project_tbAllocation> Project_tbAllocations { get; set; }
        public virtual DbSet<Project_tbAllocationEvent> Project_tbAllocationEvents { get; set; }
        public virtual DbSet<Cash_tbAssetType> Cash_tbAssetTypes { get; set; }
        public virtual DbSet<Object_tbAttribute> Object_tbAttributes { get; set; }
        public virtual DbSet<Project_tbAttribute> Project_tbAttributes { get; set; }
        public virtual DbSet<Object_tbAttributeType> Object_tbAttributeTypes { get; set; }
        public virtual DbSet<App_tbBucket> App_tbBuckets { get; set; }
        public virtual DbSet<App_tbBucketInterval> App_tbBucketIntervals { get; set; }
        public virtual DbSet<App_tbBucketType> App_tbBucketTypes { get; set; }
        public virtual DbSet<App_tbCalendar> App_tbCalendars { get; set; }
        public virtual DbSet<App_tbCalendarHoliday> App_tbCalendarHolidays { get; set; }
        public virtual DbSet<Cash_tbCategory> Cash_tbCategories { get; set; }
        public virtual DbSet<Cash_tbCategoryExp> Cash_tbCategoryExps { get; set; }
        public virtual DbSet<Cash_tbCategoryExpSyntax> Cash_tbCategoryExpSyntax { get; set; }
        public virtual DbSet<Cash_tbCategoryExprFormat> Cash_tbCategoryExprFormats { get; set; }
        public virtual DbSet<Cash_tbCategoryTotal> Cash_tbCategoryTotals { get; set; }
        public virtual DbSet<Cash_tbCategoryType> Cash_tbCategoryTypes { get; set; }
        public virtual DbSet<Cash_tbChange> Cash_tbChanges { get; set; }
        public virtual DbSet<Invoice_tbChangeLog> Invoice_tbChangeLogs { get; set; }
        public virtual DbSet<Project_tbChangeLog> Project_tbChangeLogs { get; set; }
        public virtual DbSet<Cash_tbChangeReference> Cash_tbChangeReferences { get; set; }
        public virtual DbSet<Cash_tbChangeStatus> Cash_tbChangeStatuses { get; set; }
        public virtual DbSet<Cash_tbChangeType> Cash_tbChangeTypes { get; set; }
        public virtual DbSet<Cash_tbCode> Cash_tbCodes { get; set; }
        public virtual DbSet<App_tbCodeExclusion> App_tbCodeExclusions { get; set; }
        public virtual DbSet<Cash_tbCoinType> Cash_tbCoinTypes { get; set; }
        public virtual DbSet<Subject_tbContact> Subject_tbContacts { get; set; }
        public virtual DbSet<Project_tbCostSet> Project_tbCostSets { get; set; }
        public virtual DbSet<App_tbDoc> App_tbDocs { get; set; }
        public virtual DbSet<Subject_tbDoc> Subject_tbDocs { get; set; }
        public virtual DbSet<Project_tbDoc> Project_tbDocs { get; set; }
        public virtual DbSet<App_tbDocClass> App_tbDocClasses { get; set; }
        public virtual DbSet<App_tbDocSpool> App_tbDocSpools { get; set; }
        public virtual DbSet<App_tbDocType> App_tbDocTypes { get; set; }
        public virtual DbSet<Invoice_tbEntry> Invoice_tbEntries { get; set; }
        public virtual DbSet<Cash_tbEntryType> Cash_tbEntryTypes { get; set; }
        public virtual DbSet<App_tbEth> App_tbEths { get; set; }
        public virtual DbSet<App_tbEventLog> App_tbEventLogs { get; set; }
        public virtual DbSet<App_tbEventType> App_tbEventTypes { get; set; }
        public virtual DbSet<Object_tbFlow> Object_tbFlows { get; set; }
        public virtual DbSet<Project_tbFlow> Project_tbFlows { get; set; }
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
        public virtual DbSet<Object_tbMirror> Object_tbMirrors { get; set; }
        public virtual DbSet<Cash_tbMirror> Cash_tbMirrors { get; set; }
        public virtual DbSet<Invoice_tbMirror> Invoice_tbMirrors { get; set; }
        public virtual DbSet<Invoice_tbMirrorEvent> Invoice_tbMirrorEvents { get; set; }
        public virtual DbSet<Invoice_tbMirrorItem> Invoice_tbMirrorItems { get; set; }
        public virtual DbSet<Invoice_tbMirrorReference> Invoice_tbMirrorReferences { get; set; }
        public virtual DbSet<Invoice_tbMirrorProject> Invoice_tbMirrorProjects { get; set; }
        public virtual DbSet<Cash_tbPolarity> Cash_tbPolaritys { get; set; }
        public virtual DbSet<App_tbMonth> App_tbMonths { get; set; }
        public virtual DbSet<Object_tbOp> Object_tbOps { get; set; }
        public virtual DbSet<Project_tbOp> Project_tbOps { get; set; }
        public virtual DbSet<Project_tbOpStatus> Project_tbOpStatuses { get; set; }
        public virtual DbSet<App_tbOption> App_tbOptions { get; set; }
        public virtual DbSet<Subject_tbSubject> Subject_tbSubjects { get; set; }
        public virtual DbSet<Cash_tbPayment> Cash_tbPayments { get; set; }
        public virtual DbSet<Cash_tbPaymentStatus> Cash_tbPaymentStatuses { get; set; }
        public virtual DbSet<App_tbPeriod> App_tbPeriods { get; set; }
        public virtual DbSet<Project_tbQuote> Project_tbQuotes { get; set; }
        public virtual DbSet<App_tbRecurrence> App_tbRecurrences { get; set; }
        public virtual DbSet<App_tbRegister> App_tbRegisters { get; set; }
        public virtual DbSet<App_tbRounding> App_tbRoundings { get; set; }
        public virtual DbSet<Subject_tbSector> Subject_tbSectors { get; set; }
        public virtual DbSet<Cash_tbStatus> Cash_tbStatuses { get; set; }
        public virtual DbSet<Invoice_tbStatus> Invoice_tbStatuses { get; set; }
        public virtual DbSet<Subject_tbStatus> Subject_tbStatuses { get; set; }
        public virtual DbSet<Project_tbStatus> Project_tbStatuses { get; set; }
        public virtual DbSet<Object_tbSyncType> Object_tbSyncTypes { get; set; }
        public virtual DbSet<Invoice_tbProject> Invoice_tbProjects { get; set; }
        public virtual DbSet<Project_tbProject> Project_tbProjects { get; set; }
        public virtual DbSet<App_tbTaxCode> App_tbTaxCodes { get; set; }
        public virtual DbSet<Cash_tbTaxType> Cash_tbTaxTypes { get; set; }
        public virtual DbSet<App_tbTemplate> App_tbTemplates { get; set; }
        public virtual DbSet<App_tbText> App_tbTexts { get; set; }
        public virtual DbSet<Subject_tbTransmitStatus> Subject_tbTransmitStatuses { get; set; }
        public virtual DbSet<Cash_tbTx> Cash_tbTxs { get; set; }
        public virtual DbSet<Cash_tbTxReference> Cash_tbTxReferences { get; set; }
        public virtual DbSet<Cash_tbTxStatus> Cash_tbTxStatuses { get; set; }
        public virtual DbSet<Cash_tbType> Cash_tbTypes { get; set; }
        public virtual DbSet<Invoice_tbType> Invoice_tbTypes { get; set; }
        public virtual DbSet<Invoice_vwType> Invoice_Types { get; set; }
        public virtual DbSet<Subject_tbType> Subject_tbTypes { get; set; }
        public virtual DbSet<App_tbUoc> App_tbUocs { get; set; }
        public virtual DbSet<App_tbUom> App_tbUoms { get; set; }
        public virtual DbSet<Usr_tbUser> Usr_tbUsers { get; set; }
        public virtual DbSet<App_tbYear> App_tbYears { get; set; }
        public virtual DbSet<App_vwYear> App_Years { get; set; }
        public virtual DbSet<App_tbYearPeriod> App_tbYearPeriods { get; set; }
        public virtual DbSet<Web_tbTemplate> Web_tbTemplates { get; set; }
        public virtual DbSet<Web_tbTemplateStatus> Web_tbTemplateStatuses { get; set; }
        public virtual DbSet<Web_tbTemplateImage> Web_tbTemplateImages { get; set; }
        public virtual DbSet<Web_tbTemplateInvoice> Web_tbTemplateInvoices { get; set; }
        public virtual DbSet<Web_tbImage> Web_tbImages { get; set; }
        public virtual DbSet<Web_tbAttachment> Web_tbAttachments { get; set; }
        public virtual DbSet<Web_tbAttachmentInvoice> Web_tbAttachmentInvoices { get; set; }


        #endregion

        #region Asp.Net
        public virtual DbSet<AspNet_UserRegistration> AspNet_UserRegistrations { get; set; }

        #endregion

        #region Views
        public virtual DbSet<Cash_vwCategoryPrimaryParent> Cash_vwCategoryPrimaryParents { get; set; }
        public virtual DbSet<Subject_vwSubjectLookup> Subject_SubjectLookup { get; set; }
        public virtual DbSet<Subject_vwSubjectLookupAll> Subject_SubjectLookupAll { get; set; }
        public virtual DbSet<Subject_vwEmailAddress> Subject_EmailAddresses { get; set; }
        public virtual DbSet<Subject_vwSubjectSource> Subject_SubjectSources { get; set; }
        public virtual DbSet<Cash_vwAccountStatement> Cash_AccountStatements { get; set; }
        public virtual DbSet<Cash_vwAccountStatementListing> Cash_AccountStatementListings { get; set; }
        public virtual DbSet<Cash_vwBalanceSheet> Cash_BalanceSheet { get; set; }
        public virtual DbSet<Cash_vwProfitAndLossByPeriod> Cash_ProfitAndLossByMonth { get; set; }
        public virtual DbSet<Cash_vwProfitAndLossByYear> Cash_ProfitAndLossByYear { get; set; }
        public virtual DbSet<Invoice_vwAccountsMode> Invoice_AccountsMode { get; set; }
        public virtual DbSet<App_vwHost> App_Host { get; set; }
        public virtual DbSet<App_tbHost> App_tbHosts { get; set; }
        public virtual DbSet<Project_vwActiveDatum> Project_ActiveData { get; set; }
        public virtual DbSet<App_vwActivePeriod> App_ActivePeriods { get; set; }
        public virtual DbSet<Project_vwActiveStatusCode> Project_ActiveStatusCodes { get; set; }
        public virtual DbSet<Invoice_vwAgedDebtPurchase> Invoice_AgedDebtPurchases { get; set; }
        public virtual DbSet<Invoice_vwAgedDebtSale> Invoice_AgedDebtSales { get; set; }
        public virtual DbSet<Project_vwAllocationSvD> Project_AllocationSvD { get; set; }
        public virtual DbSet<Subject_vwAreaCode> Subject_AreaCodes { get; set; }
        public virtual DbSet<Subject_vwAssetStatementAudit> Subject_AssetStatementAudits { get; set; }
        public virtual DbSet<Project_vwAttributeDescription> Project_AttributeDescriptions { get; set; }
        public virtual DbSet<Project_vwAttributesForOrder> Project_AttributesForOrders { get; set; }
        public virtual DbSet<Project_vwAttributesForQuote> Project_AttributesForQuotes { get; set; }
        public virtual DbSet<Subject_vwBalanceSheetAudit> Subject_BalanceSheetAudits { get; set; }
        public virtual DbSet<Cash_vwBankCashCode> Cash_BankCashCodes { get; set; }
        public virtual DbSet<Cash_vwBudget> Cash_Budget { get; set; }
        public virtual DbSet<Cash_vwBudgetDataEntry> Cash_BudgetDataEntries { get; set; }
        public virtual DbSet<Object_wCandidateCashCode> Object_CandidateCashCodes { get; set; }
        public virtual DbSet<App_vwCandidateCategoryCode> App_CandidateCategoryCodes { get; set; }
        public virtual DbSet<Invoice_vwCandidateCredit> Invoice_CandidateCredits { get; set; }
        public virtual DbSet<Invoice_vwCandidateDebit> Invoice_CandidateDebits { get; set; }
        public virtual DbSet<App_vwCandidateHomeAccount> App_CandidateHomeAccounts { get; set; }
        public virtual DbSet<Invoice_vwCandidatePurchase> Invoice_CandidatePurchases { get; set; }
        public virtual DbSet<Invoice_vwCandidateSale> Invoice_CandidateSales { get; set; }
        public virtual DbSet<Subject_vwCashAccountAsset> Subject_CashAccountAssets { get; set; }
        public virtual DbSet<Subject_vwCashAccount> Subject_CashAccounts { get; set; }
        public virtual DbSet<Cash_vwCashFlowType> Cash_FlowTypes { get; set; }
        public virtual DbSet<Cash_vwFlowCategory> Cash_FlowCategories { get; set; }
        public virtual DbSet<Cash_vwFlowCategoryByPeriod> Cash_FlowCategoryByPeriods { get; set; }
        public virtual DbSet<Cash_vwFlowCategoryByYear> Cash_FlowCategoryByYears { get; set; }
        public virtual DbSet<Cash_vwCategoryBudget> Cash_CategoryBudget { get; set; }
        public virtual DbSet<Cash_vwCategoryTotal> Cash_CategoryTotals { get; set; }
        public virtual DbSet<Cash_vwCategoryTotalCandidate> Cash_CategoryTotalCandidates { get; set; }
        public virtual DbSet<Cash_vwCategoryTrade> Cash_CategoryTrades { get; set; }
        public virtual DbSet<Invoice_vwChangeLog> Invoice_ChangeLog { get; set; }
        public virtual DbSet<Project_vwChangeLog> Project_ChangeLog { get; set; }
        public virtual DbSet<Object_vwCode> Object_Codes { get; set; }
        public virtual DbSet<Cash_vwCodeLookup> Cash_CodeLookup { get; set; }
        public virtual DbSet<Cash_vwCode> Cash_Codes { get; set; }
        public virtual DbSet<Subject_vwCompanyHeader> Subject_CompanyHeaders { get; set; }
        public virtual DbSet<Subject_vwCompanyLogo> Subject_CompanyLogos { get; set; }
        public virtual DbSet<Subject_vwContact> Subject_Contacts { get; set; }
        public virtual DbSet<Subject_vwCurrentAccount> Subject_CurrentAccounts { get; set; }

        public virtual DbSet<Subject_vwAddressList> Subject_AddressList { get; set; }
        public virtual DbSet<Project_vwCostSet> Project_CostSet { get; set; }
        public virtual DbSet<Invoice_vwCreditNoteSpool> Invoice_CreditNoteSpool { get; set; }
        public virtual DbSet<Invoice_vwCreditSpoolByItem> Invoice_CreditSpoolByItem { get; set; }
        public virtual DbSet<Subject_vwDatasheet> Subject_Datasheet { get; set; }
        public virtual DbSet<Invoice_vwDebitNoteSpool> Invoice_DebitNoteSpool { get; set; }
        public virtual DbSet<Object_vwDefaultText> Object_DefaultText { get; set; }
        public virtual DbSet<Subject_vwDepartment> Subject_Departments { get; set; }
        public virtual DbSet<Usr_vwDoc> Usr_Doc { get; set; }
        public virtual DbSet<Invoice_vwDoc> Invoice_Doc { get; set; }
        public virtual DbSet<Invoice_vwDocDetail> Invoice_DocDetails { get; set; }
        public virtual DbSet<App_vwDocCreditNote> App_DocCreditNotes { get; set; }
        public virtual DbSet<App_vwDocDebitNote> App_DocDebitNotes { get; set; }
        public virtual DbSet<App_vwDocOpenMode> App_DocOpenModes { get; set; }
        public virtual DbSet<App_vwDocPurchaseEnquiry> App_DocPurchaseEnquiries { get; set; }
        public virtual DbSet<App_vwDocPurchaseOrder> App_DocPurchaseOrders { get; set; }
        public virtual DbSet<App_vwDocQuotation> App_DocQuotations { get; set; }
        public virtual DbSet<App_vwDocSalesInvoice> App_DocSalesInvoices { get; set; }
        public virtual DbSet<App_vwDocSalesOrder> App_DocSalesOrders { get; set; }
        public virtual DbSet<App_vwEventLog> App_EventLogs { get; set; }
        public virtual DbSet<Object_vwExpenseCashCode> Object_ExpenseCashCodes { get; set; }
        public virtual DbSet<Cash_vwExternalCodesLookup> Cash_ExternalCodesLookup { get; set; }
        public virtual DbSet<Project_vwFlow> Project_Flow { get; set; }
        public virtual DbSet<App_vwGraphBankBalance> App_GraphBankBalances { get; set; }
        public virtual DbSet<App_vwGraphProjectObject> App_GraphProjectActivities { get; set; }
        public virtual DbSet<Invoice_vwHistoryCashCode> Invoice_HistoryCashCodes { get; set; }
        public virtual DbSet<Invoice_vwHistoryPurchase> Invoice_HistoryPurchases { get; set; }
        public virtual DbSet<Invoice_vwHistoryPurchaseItem> Invoice_HistoryPurchaseItems { get; set; }
        public virtual DbSet<Invoice_vwHistorySale> Invoice_HistorySales { get; set; }
        public virtual DbSet<Invoice_vwHistorySalesItem> Invoice_HistorySalesItems { get; set; }
        public virtual DbSet<App_vwIdentity> App_Identity { get; set; }
        public virtual DbSet<Object_wIncomeCashCode> VwIncomeCashCodes { get; set; }
        public virtual DbSet<Subject_vwInvoiceItem> Subject_InvoiceItems { get; set; }
        public virtual DbSet<Subject_vwInvoiceSummary> Subject_InvoiceSummaries { get; set; }
        public virtual DbSet<Subject_vwInvoiceProject> Subject_InvoiceProjects { get; set; }
        public virtual DbSet<Invoice_vwItem> Invoice_Items { get; set; }
        public virtual DbSet<Invoice_vwEntry> Invoice_Entries { get; set; }
        public virtual DbSet<Subject_vwJobTitle> Subject_JobTitles { get; set; }
        public virtual DbSet<Subject_vwListActive> Subject_ListActive { get; set; }
        public virtual DbSet<Subject_vwListAll> Subject_ListAll { get; set; }
        public virtual DbSet<Invoice_vwMirror> Invoice_Mirrors { get; set; }
        public virtual DbSet<Invoice_vwMirrorDetail> Invoice_MirrorDetails { get; set; }
        public virtual DbSet<Invoice_vwMirrorEvent> Invoice_MirrorEvents { get; set; }
        public virtual DbSet<Subject_vwNameTitle> Subject_NameTitles { get; set; }
        public virtual DbSet<Project_vwNetworkAllocation> Project_NetworkAllocations { get; set; }
        public virtual DbSet<Invoice_vwNetworkChangeLog> Invoice_NetworkChangeLog { get; set; }
        public virtual DbSet<Project_vwNetworkChangeLog> Project_NetworkChangeLogs { get; set; }
        public virtual DbSet<Project_vwNetworkEvent> Project_NetworkEvents { get; set; }
        public virtual DbSet<Project_vwNetworkEventLog> Project_NetworkEventLog { get; set; }
        public virtual DbSet<Project_vwNetworkQuotation> Project_NetworkQuotations { get; set; }
        public virtual DbSet<Project_vwOp> Project_Ops { get; set; }
        public virtual DbSet<Cash_vwPayment> Cash_Payments { get; set; }
        public virtual DbSet<Subject_vwPaymentTerm> Subject_PaymentTerms { get; set; }
        public virtual DbSet<Cash_vwPaymentsListing> Cash_PaymentsListing { get; set; }
        public virtual DbSet<Cash_vwPaymentsUnposted> Cash_PaymentsUnposted { get; set; }
        public virtual DbSet<App_vwPeriod> App_Periods { get; set; }
        public virtual DbSet<App_vwPeriodEndListing> App_PeriodEndListings { get; set; }
        public virtual DbSet<Project_vwProfit> Project_Profit { get; set; }
        public virtual DbSet<Project_vwProfitToDate> Project_ProfitToDate { get; set; }
        public virtual DbSet<Project_vwPurchase> Project_Purchases { get; set; }
        public virtual DbSet<Project_vwPurchaseEnquiryDeliverySpool> Project_PurchaseEnquiryDeliverySpool { get; set; }
        public virtual DbSet<Project_vwPurchaseEnquirySpool> Project_PurchaseEnquirySpool { get; set; }
        public virtual DbSet<Project_vwPurchaseOrderDeliverySpool> Project_PurchaseOrderDeliverySpool { get; set; }
        public virtual DbSet<Project_vwPurchaseOrderSpool> Project_PurchaseOrderSpool { get; set; }
        public virtual DbSet<Project_vwQuotationSpool> Project_QuotationSpool { get; set; }
        public virtual DbSet<Project_vwQuote> Project_Quotes { get; set; }
        public virtual DbSet<Invoice_vwRegister> Invoice_Register { get; set; }
        public virtual DbSet<Invoice_vwRegisterCashCode> Invoice_RegisterCashCodes { get; set; }
        public virtual DbSet<Invoice_vwRegisterDetail> Invoice_RegisterDetails { get; set; }
        public virtual DbSet<Invoice_vwRegisterExpense> Invoice_RegisterExpenses { get; set; }
        public virtual DbSet<Invoice_vwRegisterItem> Invoice_RegisterItems { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchase> Invoice_RegisterPurchases { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchaseProject> Invoice_RegisterPurchaseProjects { get; set; }
        public virtual DbSet<Invoice_vwRegisterPurchasesOverdue> Invoice_RegisterPurchasesOverdue { get; set; }
        public virtual DbSet<Invoice_vwRegisterOverdue> Invoice_RegisterOverdue { get; set; }
        public virtual DbSet<Invoice_vwRegisterSale> Invoice_RegisterSales { get; set; }
        public virtual DbSet<Invoice_vwRegisterSaleProject> Invoice_RegisterSaleProjects { get; set; }
        public virtual DbSet<Invoice_vwRegisterSalesOverdue> Invoice_RegisterSalesOverdues { get; set; }
        public virtual DbSet<Subject_vwReserveAccount> Subject_ReserveAccounts { get; set; }
        public virtual DbSet<Project_vwSale> Project_Sales { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpool> Invoice_SalesInvoiceSpool { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpoolByObject> Invoice_SalesInvoiceSpoolByObject { get; set; }
        public virtual DbSet<Invoice_vwSalesInvoiceSpoolByItem> Invoice_SalesInvoiceSpoolByItem { get; set; }
        public virtual DbSet<Project_vwSalesOrderSpool> Project_SalesOrderSpool { get; set; }
        public virtual DbSet<Cash_vwStatement> Cash_Statement { get; set; }
        public virtual DbSet<Subject_vwStatement> Subject_Statement { get; set; }
        public virtual DbSet<Subject_vwStatementReport> Subject_StatementReport { get; set; }
        public virtual DbSet<Cash_vwStatementReserve> Cash_StatementReserves { get; set; }
        public virtual DbSet<Cash_vwStatementWhatIf> Cash_StatementWhatIf { get; set; }
        public virtual DbSet<Subject_vwStatusReport> Subject_StatusReport { get; set; }
        public virtual DbSet<Cash_vwSummary> Cash_Summary { get; set; }
        public virtual DbSet<Invoice_vwSummary> Invoice_Summary { get; set; }
        public virtual DbSet<Subject_vwProject> Subject_Projects { get; set; }
        public virtual DbSet<Project_vwProject> Project_Projects { get; set; }
        public virtual DbSet<App_vwTaxCode> App_TaxCodes { get; set; }
        public virtual DbSet<Cash_vwTaxType> App_TaxTypes { get; set; }
        public virtual DbSet<App_vwTaxCodeType> App_TaxCodeTypes { get; set; }
        public virtual DbSet<Cash_vwTaxCorpAuditAccrual> Cash_TaxCorpAuditAccruals { get; set; }
        public virtual DbSet<Cash_vwTaxCorpStatement> Cash_TaxCorpStatement { get; set; }
        public virtual DbSet<Cash_vwTaxLossesCarriedForward> Cash_TaxLossesCarriedForward { get; set; }
        public virtual DbSet<Cash_vwTaxCorpTotal> Cash_TaxCorpTotals { get; set; }
        public virtual DbSet<Invoice_vwTaxSummary> Invoice_TaxSummary { get; set; }
        public virtual DbSet<Cash_vwTaxVatAuditAccrual> Cash_TaxVatAuditAccruals { get; set; }
        public virtual DbSet<Cash_vwTaxVatAuditInvoice> Cash_TaxVatAuditInvoices { get; set; }
        public virtual DbSet<Cash_vwTaxVatDetail> Cash_TaxVatDetails { get; set; }
        public virtual DbSet<Cash_vwTaxVatStatement> Cash_TaxVatStatement { get; set; }
        public virtual DbSet<Cash_vwTaxVatSummary> Cash_TaxVatSummary { get; set; }
        public virtual DbSet<Cash_vwTaxVatTotal> Cash_TaxVatTotals { get; set; }
        public virtual DbSet<Project_vwTitle> Project_Titles { get; set; }
        public virtual DbSet<Cash_vwTransferCodeLookup> Cash_TransferCodeLookup { get; set; }
        public virtual DbSet<Cash_vwTransfersUnposted> Cash_TransfersUnposted { get; set; }
        public virtual DbSet<Subject_vwTypeLookup> Subject_TypeLookup { get; set; }
        public virtual DbSet<Object_vwUnMirrored> Object_UnMirrored { get; set; }
        public virtual DbSet<Cash_vwUnMirrored> Cash_UnMirrored { get; set; }
        public virtual DbSet<Usr_vwUserMenu> Usr_UserMenus { get; set; }
        public virtual DbSet<Usr_vwUserMenuList> Usr_UserMenuLists { get; set; }
        public virtual DbSet<Cash_vwVatcode> Cash_Vatcodes { get; set; }
        public virtual DbSet<App_vwVersion> App_Version { get; set; }
        public virtual DbSet<App_vwHomeAccount> App_HomeAccount { get; set; }
        public virtual DbSet<App_vwWarehouseSubject> App_WarehouseSubjects { get; set; }
        public virtual DbSet<App_vwWarehouseProject> App_WarehouseProjects { get; set; }
        public virtual DbSet<App_vwYearPeriod> App_YearPeriods { get; set; }
        public virtual DbSet<Usr_vwCredential> Usr_Credentials { get; set; }
        public virtual DbSet<Web_vwAttachmentInvoice> Web_AttachmentInvoices { get; set; }
        public virtual DbSet<Web_vwTemplateImage> Web_TemplateImages { get; set; }
        public virtual DbSet<Web_vwTemplateInvoice> Web_TemplateInvoices { get; set; }
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

            modelBuilder.Entity<Subject_tbAccount>(entity =>
            {
                entity.HasKey(e => e.AccountCode)
                    .HasName("PK_Subject_tbAccount");

                entity.HasIndex(e => new { e.SubjectCode, e.AccountCode }, "IX_Subject_tbAccount")
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

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subject_tbAccount_Subject_tb");

                entity.HasOne(d => d.AccountTypeCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.AccountTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subject_tbAccount_Subject_tbAccountType");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Subject_tbAccount_Cash_tbCode");

                entity.HasOne(d => d.CoinTypeCodeNavigation)
                    .WithMany(p => p.TbAccounts)
                    .HasForeignKey(d => d.CoinTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subject_tbAccount_Cash_tbCoinType");
            });

            modelBuilder.Entity<Subject_tbAccountType>(entity =>
            {
                entity.HasKey(e => e.AccountTypeCode)
                    .HasName("PK_Subject_tbAccountType");

                entity.Property(e => e.AccountTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Object_tbObject>(entity =>
            {
                entity.HasKey(e => e.ObjectCode)
                    .HasName("PK_Object_tbObjectCode")
                    .IsClustered(false);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.ProjectStatusCode).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Object_tbObject_Cash_tbCode");

                entity.HasOne(d => d.RegisterNameNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.RegisterName)
                    .HasConstraintName("FK_Object_tbObject_App_tbRegister");

                entity.HasOne(d => d.UnitOfMeasureNavigation)
                    .WithMany(p => p.TbActivities)
                    .HasForeignKey(d => d.UnitOfMeasure)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbObject_App_tbUom");
            });

            modelBuilder.Entity<Subject_tbAddress>(entity =>
            {
                entity.HasKey(e => e.AddressCode)
                    .HasName("PK_Subject_tbAddress");

                entity.HasIndex(e => new { e.SubjectCode, e.AddressCode }, "IX_Subject_tbAddress")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                ////entity.Property(e => e.RowVer)
                ////    .IsRowVersion()
                ////    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbAddresses)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Subject_tbAddress_Subject_tb");
            });

            modelBuilder.Entity<Project_tbAllocation>(entity =>
            {
                entity.HasKey(e => e.ContractAddress)
                    .HasName("PK_Project_tbAllocation");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Project_tbAllocation_SubjectCode");

                entity.HasOne(d => d.CashPolarityCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.CashPolarityCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbAllocation_CashPolarityCode");

                entity.HasOne(d => d.ProjectStatusCodeNavigation)
                    .WithMany(p => p.TbAllocations)
                    .HasForeignKey(d => d.ProjectStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbAllocation_ProjectStatusCode");
            });

            modelBuilder.Entity<Project_tbAllocationEvent>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.LogId })
                    .HasName("PK_Project_tbAllocationEvent");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.ContractAddress)
                    .HasConstraintName("FK_Project_tbAllocationEvent_tbAllocation");

                entity.HasOne(d => d.EventTypeCodeNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.EventTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbAllocationEvent_App_tbEventType");

                entity.HasOne(d => d.ProjectStatusCodeNavigation)
                    .WithMany(p => p.TbAllocationEvents)
                    .HasForeignKey(d => d.ProjectStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbAllocationEvent_Project_tbStatus");
            });

            modelBuilder.Entity<Cash_tbAssetType>(entity =>
            {
                entity.HasKey(e => e.AssetTypeCode)
                    .HasName("PK_Cash_tbAssetType");

                entity.Property(e => e.AssetTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Object_tbAttribute>(entity =>
            {
                entity.HasKey(e => new { e.ObjectCode, e.Attribute })
                    .HasName("PK_Object_tbAttribute");

                entity.HasIndex(e => e.Attribute, "IX_Object_tbAttribute")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.DefaultText, "IX_Object_tbAttribute_DefaultText")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ObjectCode, e.PrintOrder, e.Attribute }, "IX_Object_tbAttribute_OrderBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ObjectCode, e.AttributeTypeCode, e.PrintOrder }, "IX_Object_tbAttribute_Type_OrderBy")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrintOrder).HasDefaultValueSql("((10))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ObjectCodeNavigation)
                    .WithMany(p => p.TbAttributes)
                    .HasForeignKey(d => d.ObjectCode)
                    .HasConstraintName("FK_Object_tbAttribute_tbObject");

                entity.HasOne(d => d.AttributeTypeCodeNavigation)
                    .WithMany(p => p.TbAttributes)
                    .HasForeignKey(d => d.AttributeTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbAttribute_Object_tbAttributeType");
            });

            modelBuilder.Entity<Project_tbAttribute>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.Attribute })
                    .HasName("PK_Project_tbProjectAttribute");

                entity.HasIndex(e => e.ProjectCode, "IX_Project_tbAttribute")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.Attribute, e.AttributeDescription }, "IX_Project_tbAttribute_Description")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ProjectCode, e.PrintOrder, e.Attribute }, "IX_Project_tbAttribute_OrderBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ProjectCode, e.AttributeTypeCode, e.PrintOrder }, "IX_Project_tbAttribute_Type_OrderBy")
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
                    .HasConstraintName("FK_Project_tbAttribute_Object_tbAttributeType");

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbAttribute1s)
                    .HasForeignKey(d => d.ProjectCode)
                    .HasConstraintName("FK_Project_tbAttrib_Project_tb");
            });

            modelBuilder.Entity<Object_tbAttributeType>(entity =>
            {
                entity.HasKey(e => e.AttributeTypeCode)
                    .HasName("PK_Object_tbAttributeType");

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

                //entity.Property(e => e.CashPolarityCode).HasDefaultValueSql("((1))");

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

                entity.HasOne(d => d.CashPolarityCodeNavigation)
                    .WithMany(p => p.TbCategories)
                    .HasForeignKey(d => d.CashPolarityCode)
                    .HasConstraintName("FK_Cash_tbCategory_Cash_tbPolarity");

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

                entity.HasOne(d => d.CategoryCodeNavigation)
                    .WithOne(p => p.TbCategoryExp)
                    .HasForeignKey<Cash_tbCategoryExp>(d => d.CategoryCode)
                    .HasConstraintName("FK_Cash_tbCategoryExp_Cash_tbCategory");

                entity.HasOne(d => d.SyntaxTypeCodeNavigation)
                    .WithMany(p => p.TbCategoryExps)
                    .HasForeignKey(d => d.SyntaxTypeCode)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_tbCategoryExp_tbCategoryExpSyntax");
            });



            modelBuilder.Entity<Cash_tbCategoryTotal>(entity =>
            {
                entity.HasKey(e => new { e.ParentCode, e.ChildCode })
                    .HasName("PK_Cash_tbCategoryTotal");

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

            modelBuilder.Entity<Project_tbChangeLog>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.LogId })
                    .HasName("PK_Project_tbChangeLog");

                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate()))");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbProjectChangeLogs)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbChangeLog_TrasmitStatusCode");
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

            modelBuilder.Entity<Subject_tbContact>(entity =>
            {
                entity.HasKey(e => new { e.SubjectCode, e.ContactName })
                    .HasName("PK_Subject_tbContact")
                    .IsClustered(false);

                entity.HasIndex(e => e.Department, "IX_Subject_tbContactDepartment")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.JobTitle, "IX_Subject_tbContactJobTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.NameTitle, "IX_Subject_tbContactNameTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.SubjectCode, "IX_Subject_tbContact_SubjectCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.OnMailingList).HasDefaultValueSql("((1))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbContacts)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Subject_tbContact_SubjectCode");
            });

            modelBuilder.Entity<Project_tbCostSet>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.UserId })
                    .HasName("PK_Project_tbCostSet");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbCostSets)
                    .HasForeignKey(d => d.ProjectCode)
                    .HasConstraintName("FK_Project_tbCostSet_Project_tbProject");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbCostSets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Project_tbCostSet_Usr_tbUser");
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

            modelBuilder.Entity<Subject_tbDoc>(entity =>
            {
                entity.HasKey(e => new { e.SubjectCode, e.DocumentName })
                    .HasName("PK_Subject_tbDoc")
                    .IsClustered(false);

                entity.HasIndex(e => e.SubjectCode, "IX_Subject_tbDoc_SubjectCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.DocumentName, e.SubjectCode }, "IX_Subject_tbDoc_DocName_SubjectCode")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbDocs)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Subject_tbDoc_SubjectCode");
            });

            modelBuilder.Entity<Project_tbDoc>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.DocumentName })
                    .HasName("PK_Project_tbDoc");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbDocs)
                    .HasForeignKey(d => d.ProjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbDoc_Project_tb");
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
                entity.HasKey(e => new { e.SubjectCode, e.CashCode })
                    .HasName("PK_Invoice_tbEntry");

                entity.Property(e => e.InvoicedOn).HasDefaultValueSql("(CONVERT([date],getdate()))");

                //entity.Property(e => e.RowVer)
                //    .IsRowVersion()
                //    .IsConcurrencyToken();

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbEntries)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbEntry_Subject_tb");

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

            modelBuilder.Entity<Object_tbFlow>(entity =>
            {
                entity.HasKey(e => new { e.ParentCode, e.StepNumber })
                    .HasName("PK_Object_tbFlow")
                    .IsClustered(false);

                entity.HasIndex(e => new { e.ChildCode, e.ParentCode }, "IX_Object_tbFlow_ChildParent")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ParentCode, e.ChildCode }, "IX_Object_tbFlow_ParentChild")
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
                    .HasConstraintName("FK_Object_tbFlow_Object_tbChild");

                entity.HasOne(d => d.ParentCodeNavigation)
                    .WithMany(p => p.TbFlowParentCodeNavigations)
                    .HasForeignKey(d => d.ParentCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbFlow_tbObjectParent");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbObjectFlows)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbFlow_Object_tbSyncType");
            });

            modelBuilder.Entity<Project_tbFlow>(entity =>
            {
                entity.HasKey(e => new { e.ParentProjectCode, e.StepNumber })
                    .HasName("PK_Project_tbFlow");

                entity.HasIndex(e => new { e.ChildProjectCode, e.ParentProjectCode }, "IX_Project_tbFlow_ChildParent")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ParentProjectCode, e.ChildProjectCode }, "IX_Project_tbFlow_ParentChild")
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

                entity.HasOne(d => d.ChildProjectCodeNavigation)
                    .WithMany(p => p.TbFlowChildProjectCodeNavigations)
                    .HasForeignKey(d => d.ChildProjectCode)
                    .HasConstraintName("FK_Project_tbFlow_Project_tb_Child");

                entity.HasOne(d => d.ParentProjectCodeNavigation)
                    .WithMany(p => p.TbFlowParentProjectCodeNavigations)
                    .HasForeignKey(d => d.ParentProjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbFlow_Project_tb_Parent");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbProjectFlows)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbFlow_Object_tbSyncType");
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

                entity.HasIndex(e => new { e.SubjectCode, e.InvoicedOn }, "IX_Invoice_tb_SubjectCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.InvoiceStatusCode, e.InvoicedOn }, "IX_Invoice_tb_Status")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.UserId, e.InvoiceNumber }, "IX_Invoice_tb_UserId")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.DueOn).HasDefaultValueSql("(dateadd(day,(1),CONVERT([date],getdate())))");

                entity.Property(e => e.ExpectedOn).HasDefaultValueSql("(dateadd(day,(1),CONVERT([date],getdate())))");

                entity.Property(e => e.InvoicedOn).HasDefaultValueSql("(CONVERT([date],getdate()))");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbInvoices)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tb_Subject_tb");

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

            modelBuilder.Entity<Object_tbMirror>(entity =>
            {
                entity.HasKey(e => new { e.ObjectCode, e.SubjectCode, e.AllocationCode })
                    .HasName("PK_Object_tbMirror");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Object_tbMirror_tbSubject");

                entity.HasOne(d => d.ObjectCodeNavigation)
                    .WithMany(p => p.TbMirrors)
                    .HasForeignKey(d => d.ObjectCode)
                    .HasConstraintName("FK_Object_tbMirror_tbObject");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbObjectMirrors)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbMirror_tbTransmitStatus");
            });

            modelBuilder.Entity<Cash_tbMirror>(entity =>
            {
                entity.HasKey(e => new { e.CashCode, e.SubjectCode, e.ChargeCode })
                    .HasName("PK_Cash_tbMirror");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbCashMirror)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbMirror_tbSubject");

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

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbInvoiceMirror)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbMirror_tbSubject");

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

            modelBuilder.Entity<Invoice_tbMirrorProject>(entity =>
            {
                entity.HasKey(e => new { e.ContractAddress, e.ProjectCode })
                    .HasName("PK_Invoice_tbMirrorProject");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.ContractAddressNavigation)
                    .WithMany(p => p.TbMirrorProjects)
                    .HasForeignKey(d => d.ContractAddress)
                    .HasConstraintName("FK_Invoice_tbMirrorProject_ContractAddress");
            });

            modelBuilder.Entity<Cash_tbPolarity>(entity =>
            {
                entity.HasKey(e => e.CashPolarityCode)
                    .HasName("PK_Cash_tbPolarity");

                entity.Property(e => e.CashPolarityCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<App_tbMonth>(entity =>
            {
                entity.HasKey(e => e.MonthNumber)
                    .HasName("PK_App_tbMonth");

                entity.Property(e => e.MonthNumber).ValueGeneratedNever();
            });

            modelBuilder.Entity<Object_tbOp>(entity =>
            {
                entity.HasKey(e => new { e.ObjectCode, e.OperationNumber })
                    .HasName("PK_Object_tbOp");

                entity.HasIndex(e => e.Operation, "IX_Object_tbOp_Operation")
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

                entity.HasOne(d => d.ObjectCodeNavigation)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.ObjectCode)
                    .HasConstraintName("FK_Object_tbOp_tbObject");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbObjectOps)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Object_tbOp_Object_tbSyncType");
            });

            modelBuilder.Entity<Project_tbOp>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.OperationNumber })
                    .HasName("PK_Project_tbOp");

                entity.HasIndex(e => new { e.OpStatusCode, e.StartOn }, "IX_Project_tbOp_OpStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.UserId, e.OpStatusCode, e.StartOn }, "IX_Project_tbOp_UserIdOpStatus")
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
                    .HasConstraintName("FK_Project_tbOp_Project_tbOpStatus");

                entity.HasOne(d => d.SyncTypeCodeNavigation)
                    .WithMany(p => p.TbProjectOps)
                    .HasForeignKey(d => d.SyncTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbOp_Object_tbSyncType");

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.ProjectCode)
                    .HasConstraintName("FK_Project_tbOp_Project_tb");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbOps)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tbOp_Usr_tb");
            });

            modelBuilder.Entity<Project_tbOpStatus>(entity =>
            {
                entity.HasKey(e => e.OpStatusCode)
                    .HasName("PK_Project_tbOpStatus");

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

                entity.Property(e => e.TaxHorizon).HasDefaultValueSql("((90))");

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbOptionSubjectCodeNavigations)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbOptions_Subject_tb");

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
                    .HasConstraintName("FK_App_tbOptions_Subject_tbSubject");

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

                entity.HasOne(d => d.HostIdNavigation)
                    .WithMany(p => p.TbOptions)
                    .HasForeignKey(d => d.HostId)
                    .HasConstraintName("FK_App_tbOptions_App_tbHost");
            });

            modelBuilder.Entity<Subject_tbSubject>(entity =>
            {
                entity.HasKey(e => e.SubjectCode)
                    .HasName("PK_Subject_tbSubject")
                    .IsClustered(false);

                entity.HasIndex(e => e.SubjectName, "IX_Subject_tb_SubjectName")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.SubjectSource, "IX_Subject_tb_SubjectSource")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.AreaCode, "IX_Subject_tb_AreaCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.SubjectStatusCode, "IX_Subject_tb_SubjectStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.SubjectTypeCode, "IX_Subject_tb_SubjectTypeCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.PaymentTerms, "IX_Subject_tb_PaymentTerms")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.SubjectStatusCode, e.SubjectName }, "IX_Subject_tb_Status_SubjectCode")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.HasOne(d => d.AddressCodeNavigation)
                    .WithMany(p => p.TbSubjects)
                    .HasForeignKey(d => d.AddressCode)
                    .HasConstraintName("FK_Subject_tb_Subject_tbAddress");

                entity.HasOne(d => d.SubjectStatusCodeNavigation)
                    .WithMany(p => p.TbSubjects)
                    .HasForeignKey(d => d.SubjectStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tbSubject_FK00");

                entity.HasOne(d => d.SubjectTypeCodeNavigation)
                    .WithMany(p => p.TbSubjects)
                    .HasForeignKey(d => d.SubjectTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tbSubject_FK01");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbSubjects)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Subject_tb_App_tbTaxCode");

                entity.HasOne(d => d.TransmitStatusCodeNavigation)
                    .WithMany(p => p.TbSubjects)
                    .HasForeignKey(d => d.TransmitStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subject_tbSubject_tbTransmitStatus");
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

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_tbSubject");

                entity.HasOne(d => d.AccountCodeNavigation)
                    .WithMany(p => p.TbPayments)
                    .HasForeignKey(d => d.AccountCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cash_tbPayment_Subject_tbAccount");

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

                //entity.HasOne(d => d.StartOnNavigation)
                //    .WithMany(p => p.TbPeriods)
                //    .HasPrincipalKey(p => p.StartOn)
                //    .HasForeignKey(d => d.StartOn)
                //    .HasConstraintName("FK_Cash_tbPeriod_App_tbYearPeriod");
            });

            modelBuilder.Entity<Project_tbQuote>(entity =>
            {
                entity.HasKey(e => new { e.ProjectCode, e.Quantity })
                    .HasName("PK_Project_tbQuote");

                entity.Property(e => e.InsertedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.InsertedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("(suser_sname())");

                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbQuotes)
                    .HasForeignKey(d => d.ProjectCode)
                    .HasConstraintName("FK_Project_tbQuote_Project_tb");
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

            modelBuilder.Entity<Subject_tbSector>(entity =>
            {
                entity.HasKey(e => new { e.SubjectCode, e.IndustrySector })
                    .HasName("PK_Subject_tbSector");

                entity.HasIndex(e => e.IndustrySector, "IX_Subject_tbSector_IndustrySector")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbSectors)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Subject_tbSector_Subject_tb");
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

            modelBuilder.Entity<Subject_tbStatus>(entity =>
            {
                entity.HasKey(e => e.SubjectStatusCode)
                    .HasName("PK_Subject_tbStatus")
                    .IsClustered(false);

                entity.Property(e => e.SubjectStatusCode).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Project_tbStatus>(entity =>
            {
                entity.HasKey(e => e.ProjectStatusCode)
                    .HasName("PK_Project_tbStatus")
                    .IsClustered(false);

                entity.HasIndex(e => e.ProjectStatus, "IX_Project_tbStatus_ProjectStatus")
                    .IsUnique()
                    .HasFillFactor((byte)90);

                entity.Property(e => e.ProjectStatusCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Object_tbSyncType>(entity =>
            {
                entity.HasKey(e => e.SyncTypeCode)
                    .HasName("PK_Object_tbSyncType");

                entity.Property(e => e.SyncTypeCode).ValueGeneratedNever();
            });

            modelBuilder.Entity<Invoice_tbProject>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceNumber, e.ProjectCode })
                    .HasName("PK_Invoice_tbProject");

                entity.HasIndex(e => new { e.CashCode, e.InvoiceNumber }, "IX_Invoice_tbProject_CashCode")
                    .HasFillFactor((byte)90);

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbInvoiceProjects)
                    .HasForeignKey(d => d.CashCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbProject_Cash_tbCode");

                entity.HasOne(d => d.InvoiceNumberNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.InvoiceNumber)
                    .HasConstraintName("FK_Invoice_tbProject_Invoice_tb");

                entity.HasOne(d => d.ProjectCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.ProjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbProject_Project_tb");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbInvoiceProjects)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Invoice_tbProject_App_tbTaxCode");
            });

            modelBuilder.Entity<Project_tbProject>(entity =>
            {
                entity.HasKey(e => e.ProjectCode)
                    .HasName("PK_Project_tbProject");

                entity.HasIndex(e => e.SubjectCode, "IX_Project_tb_SubjectCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.SubjectCode, e.ActionOn }, "IX_Project_tb_SubjectCodeByActionOn")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.SubjectCode, e.ProjectStatusCode, e.ActionOn }, "IX_Project_tb_SubjectCodeByStatus")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ActionById, e.ProjectStatusCode, e.ActionOn }, "IX_Project_tb_ActionBy")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ActionById, "IX_Project_tb_ActionById")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ActionOn, "IX_Project_tb_ActionOn")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ProjectStatusCode, e.ActionOn, e.SubjectCode }, "IX_Project_tb_ActionOnStatus")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ObjectCode, "IX_Project_tb_ObjectCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ObjectCode, e.ProjectTitle }, "IX_Project_tb_ObjectCodeProjectTitle")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.ProjectStatusCode, e.ActionOn, e.SubjectCode }, "IX_Project_tb_ObjectStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => new { e.CashCode, e.ProjectStatusCode, e.ActionOn }, "IX_Project_tb_CashCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.ProjectStatusCode, "IX_Project_tb_ProjectStatusCode")
                    .HasFillFactor((byte)90);

                entity.HasIndex(e => e.UserId, "IX_Project_tb_UserId")
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

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.SubjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Object_tb_FK02");

                entity.HasOne(d => d.ActionBy)
                    .WithMany(p => p.TbProjectActionBys)
                    .HasForeignKey(d => d.ActionById)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tb_Usr_tb_ActionById");

                entity.HasOne(d => d.ObjectCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.ObjectCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Object_tb_FK00");

                entity.HasOne(d => d.AddressCodeFromNavigation)
                    .WithMany(p => p.TbProjectAddressCodeFromNavigations)
                    .HasForeignKey(d => d.AddressCodeFrom)
                    .HasConstraintName("FK_Project_tb_Subject_tbAddress_From");

                entity.HasOne(d => d.AddressCodeToNavigation)
                    .WithMany(p => p.TbProjectAddressCodeToNavigations)
                    .HasForeignKey(d => d.AddressCodeTo)
                    .HasConstraintName("FK_Project_tb_Subject_tbAddress_To");

                entity.HasOne(d => d.CashCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.CashCode)
                    .HasConstraintName("FK_Project_tb_Cash_tbCode");

                entity.HasOne(d => d.ProjectStatusCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.ProjectStatusCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Object_tb_FK01");

                entity.HasOne(d => d.TaxCodeNavigation)
                    .WithMany(p => p.TbProjects)
                    .HasForeignKey(d => d.TaxCode)
                    .HasConstraintName("FK_Project_tb_App_tbTaxCode");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TbProjectUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_tb_Usr_tb");
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

                entity.HasOne(d => d.SubjectCodeNavigation)
                    .WithMany(p => p.TbTaxTypes)
                    .HasForeignKey(d => d.SubjectCode)
                    .HasConstraintName("FK_Cash_tbTaxType_Subject_tb");

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

            modelBuilder.Entity<App_tbTemplate>(entity =>
            {
                entity.HasKey(e => e.TemplateName)
                    .HasName("PK_App_tbTemplateName");
            });

            modelBuilder.Entity<Subject_tbTransmitStatus>(entity =>
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


                entity.HasOne(d => d.CashPolarityCodeNavigation)
                    .WithMany(p => p.TbInvoiceType)
                    .HasForeignKey(d => d.CashPolarityCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_tbType_Cash_tbPolarity");
            });

            modelBuilder.Entity<Subject_tbType>(entity =>
            {
                entity.HasKey(e => e.SubjectTypeCode)
                    .HasName("PK_Subject_tbType")
                    .IsClustered(false);

                entity.Property(e => e.SubjectTypeCode).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CashPolarityCodeNavigation)
                    .WithMany(p => p.TbSubjectType)
                    .HasForeignKey(d => d.CashPolarityCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subject_tbType_Cash_tbPolarity");
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

                entity.Property(e => e.NextProjectNumber).HasDefaultValueSql("((1))");

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

                entity.HasOne(d => d.StartMonthNavigation)
                    .WithMany(p => p.TbYears)
                    .HasForeignKey(d => d.StartMonth)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_App_tbYear_App_tbMonth");
            });

            modelBuilder.Entity<App_tbYearPeriod>(entity =>
            {
                entity.HasKey(e => new { e.YearNumber, e.MonthNumber })
                    .HasName("IX_App_tbYearPeriod_Year_MonthNumber");


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

            modelBuilder.Entity<Web_tbTemplate>(entity =>
            {
                entity.HasKey(e => e.TemplateId)
                    .HasName("PK_Web_tbTemplate");
            });

            modelBuilder.Entity<Web_tbAttachment>(entity =>
            {
                entity.HasKey(e => e.AttachmentId)
                    .HasName("PK_Web_tbAttachment");
            });

            modelBuilder.Entity<Web_tbTemplateImage>(entity =>
            {
                entity.HasKey(e => new { e.TemplateId, e.ImageTag });

                entity.HasOne(d => d.ImageTagNavigation)
                    .WithMany(p => p.tbTemplateImages)
                    .HasForeignKey(d => d.ImageTag)
                    .HasConstraintName("FK_tbTemplateImage_tbImage");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.tbTemplateImages)
                    .HasForeignKey(d => d.TemplateId)
                    .HasConstraintName("FK_tbTemplateImage_tbTemplate");
            });

            modelBuilder.Entity<Web_tbTemplateInvoice>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceTypeCode, e.TemplateId });

                entity.HasOne(d => d.InvoiceTypeCodeNavigation)
                    .WithMany(p => p.TbTemplateInvoices)
                    .HasForeignKey(d => d.InvoiceTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbTemplateInvoice_tbType");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.tbTemplateInvoices)
                    .HasForeignKey(d => d.TemplateId)
                    .HasConstraintName("FK_tbTemplateInvoice_tbTemplate");
            });

            modelBuilder.Entity<Web_tbImage>(entity =>
            {
                entity.HasKey(e => e.ImageTag)
                    .HasName("PK_Web_tbImage");
            });

            modelBuilder.Entity<Web_tbAttachmentInvoice>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceTypeCode, e.AttachmentId })
                    .HasName("PK_tbInvoiceAttachment");

                entity.HasOne(d => d.Attachment)
                    .WithMany(p => p.TbAttachmentInvoices)
                    .HasForeignKey(d => d.AttachmentId)
                    .HasConstraintName("FK_tbAttachmentInvoice_tbAttachment");

                entity.HasOne(d => d.InvoiceTypeCodeNavigation)
                    .WithMany(p => p.TbAttachmentInvoices)
                    .HasForeignKey(d => d.InvoiceTypeCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbAttachmentInvoice_tbType");
            });

            modelBuilder.Entity<Subject_vwSubjectLookup>(entity =>
            {
                entity.HasKey(e => new { e.SubjectCode });
                entity.ToView("vwSubjectLookup", "Subject");
            });

            modelBuilder.Entity<Subject_vwSubjectLookupAll>(entity =>
            {
                entity.HasKey(e => new { e.SubjectCode });
                entity.ToView("vwSubjectLookupAll", "Subject");
            });

            modelBuilder.Entity<Subject_vwEmailAddress>(entity =>
            {
                entity.ToView("vwEmailAddresses", "Subject");
            });

            modelBuilder.Entity<Subject_vwSubjectSource>(entity =>
            {
                entity.ToView("vwSubjectSources", "Subject");
            });

            modelBuilder.Entity<App_vwYear>(entity =>
            {
                entity.ToView("vwYears", "App");
            });

            modelBuilder.Entity<Cash_vwAccountStatement>(entity =>
            {
                entity.ToView("vwAccountStatement", "Cash");
            });

            modelBuilder.Entity<Cash_vwAccountStatementListing>(entity =>
            {
                entity.ToView("vwAccountStatementListing", "Cash");
            });

            modelBuilder.Entity<Invoice_vwType>(entity =>
            {
                entity.HasKey(e => e.InvoiceTypeCode);
                entity.ToView("vwTypes", "Invoice");
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

            modelBuilder.Entity<Project_vwActiveDatum>(entity =>
            {
                entity.ToView("vwActiveData", "Project");
            });

            modelBuilder.Entity<App_vwActivePeriod>(entity =>
            {
                entity.ToView("vwActivePeriod", "App");
            });

            modelBuilder.Entity<Project_vwActiveStatusCode>(entity =>
            {
                entity.ToView("vwActiveStatusCodes", "Project");
            });

            modelBuilder.Entity<App_vwHost>(entity =>
            {
                entity.ToView("vwHost", "App");
            });

            modelBuilder.Entity<App_tbHost>(entity =>
            {
                entity.HasKey(e => e.HostId)
                    .HasName("PK_App_tbHost");
            });

            modelBuilder.Entity<Invoice_vwAgedDebtPurchase>(entity =>
            {
                entity.ToView("vwAgedDebtPurchases", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwAgedDebtSale>(entity =>
            {
                entity.ToView("vwAgedDebtSales", "Invoice");
            });

            modelBuilder.Entity<Project_vwAllocationSvD>(entity =>
            {
                entity.ToView("vwAllocationSvD", "Project");
            });

            modelBuilder.Entity<Subject_vwAreaCode>(entity =>
            {
                entity.ToView("vwAreaCodes", "Subject");
            });

            modelBuilder.Entity<Subject_vwAssetStatementAudit>(entity =>
            {
                entity.ToView("vwAssetStatementAudit", "Subject");
            });

            modelBuilder.Entity<Project_vwAttributeDescription>(entity =>
            {
                entity.ToView("vwAttributeDescriptions", "Project");
            });

            modelBuilder.Entity<Project_vwAttributesForOrder>(entity =>
            {
                entity.ToView("vwAttributesForOrder", "Project");
            });

            modelBuilder.Entity<Project_vwAttributesForQuote>(entity =>
            {
                entity.ToView("vwAttributesForQuote", "Project");
            });

            modelBuilder.Entity<Cash_vwProfitAndLossByPeriod>(entity =>
            {
                entity.ToView("vwProfitAndLossByPeriod", "Cash");
            });

            modelBuilder.Entity<Cash_vwProfitAndLossByYear>(entity =>
            {
                entity.ToView("vwProfitAndLossByYear", "Cash");
            });

            modelBuilder.Entity<Cash_vwBalanceSheet>(entity =>
            {
                entity.ToView("vwBalanceSheet", "Cash");
            });

            modelBuilder.Entity<Subject_vwBalanceSheetAudit>(entity =>
            {
                entity.ToView("vwBalanceSheetAudit", "Subject");
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

            modelBuilder.Entity<Object_wCandidateCashCode>(entity =>
            {
                entity.ToView("vwCandidateCashCodes", "Object");
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

            modelBuilder.Entity<Subject_vwCashAccountAsset>(entity =>
            {
                entity.ToView("vwCashAccountAssets", "Subject");
            });

            modelBuilder.Entity<Subject_vwCashAccount>(entity =>
            {
                entity.ToView("vwCashAccounts", "Subject");
            });

            modelBuilder.Entity<Cash_vwCashFlowType>(entity =>
            {
                entity.ToView("vwCashFlowTypes", "Cash");
            });

            modelBuilder.Entity<Cash_vwFlowCategory>(entity =>
            {
                entity.ToView("vwFlowCategories", "Cash");
            });

            modelBuilder.Entity<Cash_vwFlowCategoryByPeriod>(entity =>
            {
                entity.ToView("vwFlowCategoryByPeriod", "Cash");
            });

            modelBuilder.Entity<Cash_vwFlowCategoryByYear>(entity =>
            {
                entity.ToView("vwFlowCategoryByYear", "Cash");
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

            modelBuilder.Entity<Project_vwChangeLog>(entity =>
            {
                entity.ToView("vwChangeLog", "Project");
            });

            modelBuilder.Entity<Object_vwCode>(entity =>
            {
                entity.ToView("vwCodes", "Object");
            });

            modelBuilder.Entity<Cash_vwCodeLookup>(entity =>
            {
                entity.HasKey(e => new { e.CashCode });
                entity.ToView("vwCodeLookup", "Cash");
            });

            modelBuilder.Entity<Cash_vwCode>(entity =>
            {
                entity.HasKey(e => new { e.CashCode });
                entity.ToView("vwCode", "Cash");
            });

            modelBuilder.Entity<Subject_vwCompanyHeader>(entity =>
            {
                entity.ToView("vwCompanyHeader", "Subject");
            });

            modelBuilder.Entity<Subject_vwCompanyLogo>(entity =>
            {
                entity.ToView("vwCompanyLogo", "Subject");
            });

            modelBuilder.Entity<Subject_vwContact>(entity =>
            {
                entity.ToView("vwContacts", "Subject");
            });

            modelBuilder.Entity<Subject_vwAddressList>(entity =>
            {
                entity.ToView("vwAddressList", "Subject");
            });

            modelBuilder.Entity<Project_vwCostSet>(entity =>
            {
                entity.ToView("vwCostSet", "Project");

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


            modelBuilder.Entity<Subject_vwDatasheet>(entity =>
            {
                entity.ToView("vwDatasheet", "Subject");
            });

            modelBuilder.Entity<Invoice_vwDebitNoteSpool>(entity =>
            {
                entity.ToView("vwDebitNoteSpool", "Invoice");
            });

            modelBuilder.Entity<Object_vwDefaultText>(entity =>
            {
                entity.ToView("vwDefaultText", "Object");
            });

            modelBuilder.Entity<Subject_vwDepartment>(entity =>
            {
                entity.ToView("vwDepartments", "Subject");
            });

            modelBuilder.Entity<Invoice_vwDoc>(entity =>
            {
                entity.ToView("vwDoc", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwDocDetail>(entity =>
            {
                entity.ToView("vwDocDetails", "Invoice");
            });

            modelBuilder.Entity<Usr_vwDoc>(entity =>
            {
                entity.ToView("vwDoc", "Usr");
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

            });

            modelBuilder.Entity<Object_vwExpenseCashCode>(entity =>
            {
                entity.ToView("vwExpenseCashCodes", "Object");
            });

            modelBuilder.Entity<Cash_vwExternalCodesLookup>(entity =>
            {
                entity.ToView("vwExternalCodesLookup", "Cash");
            });

            modelBuilder.Entity<Project_vwFlow>(entity =>
            {
                entity.ToView("vwFlow", "Project");
            });

            modelBuilder.Entity<App_vwGraphBankBalance>(entity =>
            {
                entity.ToView("vwGraphBankBalance", "App");
            });

            modelBuilder.Entity<App_vwGraphProjectObject>(entity =>
            {
                entity.ToView("vwGraphProjectObject", "App");
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

            modelBuilder.Entity<Object_wIncomeCashCode>(entity =>
            {
                entity.ToView("vwIncomeCashCodes", "Object");
            });

            modelBuilder.Entity<Subject_vwInvoiceItem>(entity =>
            {
                entity.ToView("vwInvoiceItems", "Subject");
            });

            modelBuilder.Entity<Subject_vwInvoiceSummary>(entity =>
            {
                entity.ToView("vwInvoiceSummary", "Subject");
            });

            modelBuilder.Entity<Subject_vwInvoiceProject>(entity =>
            {
                entity.ToView("vwInvoiceProjects", "Subject");
            });

            modelBuilder.Entity<Invoice_vwItem>(entity =>
            {
                entity.ToView("vwItems", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwEntry>(entity =>
            {
                entity.ToView("vwEntry", "Invoice");
            });

            modelBuilder.Entity<Subject_vwJobTitle>(entity =>
            {
                entity.ToView("vwJobTitles", "Subject");
            });

            modelBuilder.Entity<Subject_vwListActive>(entity =>
            {
                entity.ToView("vwListActive", "Subject");
            });

            modelBuilder.Entity<Subject_vwListAll>(entity =>
            {
                entity.ToView("vwListAll", "Subject");
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


            modelBuilder.Entity<Project_vwNetworkAllocation>(entity =>
            {
                entity.ToView("vwNetworkAllocations", "Project");

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

            modelBuilder.Entity<Project_vwNetworkChangeLog>(entity =>
            {
                entity.ToView("vwNetworkChangeLog", "Project");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Project_vwNetworkEvent>(entity =>
            {
                entity.ToView("vwNetworkEvents", "Project");
            });

            modelBuilder.Entity<Project_vwNetworkEventLog>(entity =>
            {
                entity.ToView("vwNetworkEventLog", "Project");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Project_vwNetworkQuotation>(entity =>
            {
                entity.ToView("vwNetworkQuotations", "Project");
            });

            modelBuilder.Entity<Project_vwOp>(entity =>
            {
                entity.ToView("vwOps", "Project");

                entity.Property(e => e.OpRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.ProjectRowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Cash_vwPayment>(entity =>
            {
                entity.ToView("vwPayments", "Cash");
            });

            modelBuilder.Entity<Subject_vwPaymentTerm>(entity =>
            {
                entity.ToView("vwPaymentTerms", "Subject");
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
            });

            modelBuilder.Entity<App_vwPeriodEndListing>(entity =>
            {
                entity.ToView("vwPeriodEndListing", "App");
            });

            modelBuilder.Entity<Project_vwProfit>(entity =>
            {
                entity.ToView("vwProfit", "Project");
            });

            modelBuilder.Entity<Project_vwProfitToDate>(entity =>
            {
                entity.ToView("vwProfitToDate", "Project");
            });

            modelBuilder.Entity<Project_vwPurchase>(entity =>
            {
                entity.ToView("vwPurchases", "Project");
            });

            modelBuilder.Entity<Project_vwPurchaseEnquiryDeliverySpool>(entity =>
            {
                entity.ToView("vwPurchaseEnquiryDeliverySpool", "Project");
            });

            modelBuilder.Entity<Project_vwPurchaseEnquirySpool>(entity =>
            {
                entity.ToView("vwPurchaseEnquirySpool", "Project");
            });

            modelBuilder.Entity<Project_vwPurchaseOrderDeliverySpool>(entity =>
            {
                entity.ToView("vwPurchaseOrderDeliverySpool", "Project");
            });

            modelBuilder.Entity<Project_vwPurchaseOrderSpool>(entity =>
            {
                entity.ToView("vwPurchaseOrderSpool", "Project");
            });

            modelBuilder.Entity<Project_vwQuotationSpool>(entity =>
            {
                entity.ToView("vwQuotationSpool", "Project");
            });

            modelBuilder.Entity<Project_vwQuote>(entity =>
            {
                entity.ToView("vwQuotes", "Project");

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

            modelBuilder.Entity<Invoice_vwRegisterPurchaseProject>(entity =>
            {
                entity.ToView("vwRegisterPurchaseProjects", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterPurchasesOverdue>(entity =>
            {
                entity.ToView("vwRegisterPurchasesOverdue", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterOverdue>(entity =>
            {
                entity.ToView("vwRegisterOverdue", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSale>(entity =>
            {
                entity.ToView("vwRegisterSales", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSaleProject>(entity =>
            {
                entity.ToView("vwRegisterSaleProjects", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwRegisterSalesOverdue>(entity =>
            {
                entity.ToView("vwRegisterSalesOverdue", "Invoice");
            });

            modelBuilder.Entity<Subject_vwReserveAccount>(entity =>
            {
                entity.ToView("vwReserveAccount", "Cash");
            });

            modelBuilder.Entity<Subject_vwCurrentAccount>(entity =>
            {
                entity.ToView("vwCurrentAccount", "Cash");
            });

            modelBuilder.Entity<Project_vwSale>(entity =>
            {
                entity.ToView("vwSales", "Project");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpool>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpool", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpoolByObject>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpoolByObject", "Invoice");
            });

            modelBuilder.Entity<Invoice_vwSalesInvoiceSpoolByItem>(entity =>
            {
                entity.ToView("vwSalesInvoiceSpoolByItem", "Invoice");
            });

            modelBuilder.Entity<Project_vwSalesOrderSpool>(entity =>
            {
                entity.ToView("vwSalesOrderSpool", "Project");
            });

            modelBuilder.Entity<Cash_vwStatement>(entity =>
            {
                entity.ToView("vwStatement", "Cash");
            });

            modelBuilder.Entity<Subject_vwStatement>(entity =>
            {
                entity.ToView("vwStatement", "Subject");
            });

            modelBuilder.Entity<Subject_vwStatementReport>(entity =>
            {
                entity.ToView("vwStatementReport", "Subject");
            });

            modelBuilder.Entity<Cash_vwStatementReserve>(entity =>
            {
                entity.ToView("vwStatementReserves", "Cash");
            });

            modelBuilder.Entity<Cash_vwStatementWhatIf>(entity =>
            {
                entity.ToView("vwStatementWhatIf", "Cash");
            });

            modelBuilder.Entity<Subject_vwStatusReport>(entity =>
            {
                entity.ToView("vwStatusReport", "Subject");
            });

            modelBuilder.Entity<Cash_vwSummary>(entity =>
            {
                entity.ToView("vwSummary", "Cash");
            });

            modelBuilder.Entity<Invoice_vwSummary>(entity =>
            {
                entity.ToView("vwSummary", "Invoice");
            });

            modelBuilder.Entity<Subject_vwProject>(entity =>
            {
                entity.ToView("vwProjects", "Subject");
            });

            modelBuilder.Entity<Project_vwProject>(entity =>
            {
                entity.ToView("vwProjects", "Project");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwTaxCode>(entity =>
            {
                entity.HasKey(e => new { e.TaxCode });
                entity.ToView("vwTaxCodes", "App");
            });

            modelBuilder.Entity<Cash_vwTaxType>(entity =>
            {
                entity.HasKey(e => new { e.TaxTypeCode });
                entity.ToView("vwTaxTypes", "Cash");
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

            modelBuilder.Entity<Cash_vwTaxLossesCarriedForward>(entity =>
            {
                entity.ToView("vwTaxLossesCarriedForward", "Cash");
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

            modelBuilder.Entity<Project_vwTitle>(entity =>
            {
                entity.ToView("vwTitles", "Project");
            });

            modelBuilder.Entity<Cash_vwTransferCodeLookup>(entity =>
            {
                entity.ToView("vwTransferCodeLookup", "Cash");                
            });

            modelBuilder.Entity<Cash_vwTransfersUnposted>(entity =>
            {
                entity.HasKey(e => e.PaymentCode);
            });

            modelBuilder.Entity<Subject_vwTypeLookup>(entity =>
            {
                entity.ToView("vwTypeLookup", "Subject");
            });

            modelBuilder.Entity<Object_vwUnMirrored>(entity =>
            {
                entity.ToView("vwUnMirrored", "Object");
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

            modelBuilder.Entity<App_vwWarehouseSubject>(entity =>
            {
                entity.ToView("vwWarehouseSubject", "App");

                entity.Property(e => e.RowVer)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<App_vwWarehouseProject>(entity =>
            {
                entity.ToView("vwWarehouseProject", "App");

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

            modelBuilder.Entity<Web_vwAttachmentInvoice>(entity =>
            {
                entity.ToView("vwAttachmentInvoices", "Web");
            });

            modelBuilder.Entity<Web_vwTemplateImage>(entity =>
            {
                entity.ToView("vwTemplateImages", "Web");
            });

            modelBuilder.Entity<Web_vwTemplateInvoice>(entity =>
            {
                entity.ToView("vwTemplateInvoices", "Web");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        #endregion


    }
}
