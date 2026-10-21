internal static class TemplateBootstrapTests
{
    public static int Run(string root)
    {
        var assertions = 0;

        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition)
                throw new InvalidOperationException(message);
        }

        var templateManager = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Mail", "TemplateManager.cs"));
        var setupPanel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Pages", "Admin", "Manager", "Components", "SetupPanel.razor"));
        var syntheticHandler = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "AppServices", "Execution", "SyntheticDatasetExecutionHandler.cs"));
        var fileTransferPanel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Pages", "Admin", "Manager", "Components", "FileTransferPanel.razor"));

        Assert(templateManager.Contains("public async Task InitialiseBootstrap()", StringComparison.Ordinal)
               && templateManager.Contains("invoice_income_standard.html", StringComparison.Ordinal)
               && templateManager.Contains("invoice_expense_standard.html", StringComparison.Ordinal)
               && templateManager.Contains("support_request.html", StringComparison.Ordinal)
               && templateManager.Contains("user_registration.html", StringComparison.Ordinal)
               && templateManager.Contains("user_registration_confirm.html", StringComparison.Ordinal)
               && templateManager.Contains("user_registration_admin_notify.html", StringComparison.Ordinal)
               && templateManager.Contains("company_logo.png", StringComparison.Ordinal)
               && templateManager.Contains("(NodeEnum.InvoiceType.SalesInvoice, incomeTemplateFileName)", StringComparison.Ordinal)
               && templateManager.Contains("(NodeEnum.InvoiceType.DebitNote, incomeTemplateFileName)", StringComparison.Ordinal)
               && templateManager.Contains("(NodeEnum.InvoiceType.PurchaseInvoice, expenseTemplateFileName)", StringComparison.Ordinal)
               && templateManager.Contains("(NodeEnum.InvoiceType.CreditNote, expenseTemplateFileName)", StringComparison.Ordinal)
               && templateManager.Contains("item.InvoiceTypeCode == invoiceTypeCode", StringComparison.Ordinal)
               && !templateManager.Contains("item.InvoiceTypeCode == invoiceTypeCode && item.TemplateId == templateId", StringComparison.Ordinal),
            "The tenant bootstrap no longer defines the complete standard invoice-template defaults.");

        Assert(templateManager.Contains("options.SupportRequestTemplateId ??= templates[supportTemplateFileName].TemplateId", StringComparison.Ordinal)
               && templateManager.Contains("options.UserRegistrationTemplateId ??= templates[userRegistrationTemplateFileName].TemplateId", StringComparison.Ordinal)
               && templateManager.Contains("options.UserRegistrationConfirmTemplateId ??= templates[userRegistrationConfirmTemplateFileName].TemplateId", StringComparison.Ordinal)
               && templateManager.Contains("options.UserRegistrationAdminNotifyTemplateId ??= templates[userRegistrationAdminNotifyTemplateFileName].TemplateId", StringComparison.Ordinal),
            "The tenant bootstrap no longer defines all four tenant-local system-template defaults.");

        Assert(setupPanel.Contains("templateManager.InitialiseBootstrap()", StringComparison.Ordinal)
               && syntheticHandler.Contains("templateManager.InitialiseBootstrap()", StringComparison.Ordinal),
            "A business bootstrap path no longer initializes the standard HTML template assignments.");

        Assert(fileTransferPanel.Contains("await manager.Initialise()", StringComparison.Ordinal)
               && !fileTransferPanel.Contains("InitialiseBootstrap()", StringComparison.Ordinal),
            "File Transfer synchronization must reconcile files without restoring bootstrap defaults over tenant choices.");

        return assertions;
    }
}
