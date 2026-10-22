using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

internal static class PlatformEmailTests
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

        Assert(PlatformEmailOptions.IsValidHostName("smtp.example.test"),
            "A valid platform SMTP host name was rejected.");
        Assert(!PlatformEmailOptions.IsValidHostName("https://smtp.example.test")
               && !PlatformEmailOptions.IsValidHostName("not an endpoint"),
            "A URL or malformed platform SMTP host name was accepted.");
        Assert(PlatformEmailOptions.IsValidEmailAddress("donotreply@example.test")
               && !PlatformEmailOptions.IsValidEmailAddress("not-an-address"),
            "Platform sender-address validation is not enforcing an email address.");

        var validOptions = new PlatformEmailOptions
        {
            Enabled = true,
            HostName = "smtp.example.test",
            Port = 587,
            UserName = "notifications@example.test",
            Password = "test-only-password",
            SenderAddress = "notifications@example.test",
            SenderName = "Trade Control",
            Security = PlatformEmailSecurity.StartTls
        };
        Assert(validOptions.IsValid(),
            "A complete authenticated SMTP configuration was rejected.");
        validOptions.Password = string.Empty;
        Assert(!validOptions.IsValid(),
            "Platform SMTP was accepted without its Key Vault-backed password.");

        var templateManager = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Mail", "TemplateManager.cs"));
        var invoiceStart = templateManager.IndexOf(
            "public async Task<MailDocument> GetInvoice", StringComparison.Ordinal);
        var invoiceEnd = templateManager.IndexOf(
            "public async Task<MailText> GetText", invoiceStart, StringComparison.Ordinal);
        var invoiceFactory = templateManager[invoiceStart..invoiceEnd];
        Assert(!invoiceFactory.Contains("HasMailHost", StringComparison.Ordinal)
               && !invoiceFactory.Contains("MailHost()", StringComparison.Ordinal),
            "Invoice document rendering has become coupled to email delivery configuration.");

        var hostPanel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Pages", "Admin", "Manager", "Components", "HostPanel.razor"));
        Assert(hostPanel.Contains("System-managed email is active", StringComparison.Ordinal)
               && hostPanel.Contains("System-managed email remains active", StringComparison.Ordinal),
            "Mail Host does not explain the system-managed delivery route.");
        Assert(hostPanel.Contains("new TestMailSender(PlatformEmailSender)", StringComparison.Ordinal)
               && hostPanel.Contains("mailSettings is null && !PlatformEmailSender.IsConfigured", StringComparison.Ordinal),
            "Mail Host test email does not fall back to the platform delivery route.");

        var registrationPage = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Areas", "Identity", "Pages", "Account", "Register.cshtml.cs"));
        Assert(registrationPage.Contains(
                "!nodeSettings.HasMailHost && !_platformEmailSender.IsConfigured",
                StringComparison.Ordinal),
            "Registration still requires a tenant mail host when platform email is configured.");

        var confirmEmailPage = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Areas", "Identity", "Pages", "Account", "ConfirmEmail.cshtml.cs"));
        Assert(confirmEmailPage.Contains(
                "/Admin/Manager/Index?node=Users",
                StringComparison.Ordinal)
               && !confirmEmailPage.Contains("/Admin/Users/Index", StringComparison.Ordinal),
            "The registration notification does not link to the current Users panel.");

        var adminNotificationTemplate = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "wwwroot", "content", "templates",
            "user_registration_admin_notify.html"));
        Assert(adminNotificationTemplate.Contains("href=\"[UsersUrl]\"", StringComparison.Ordinal)
               && adminNotificationTemplate.Contains("Review registration", StringComparison.Ordinal)
               && adminNotificationTemplate.Contains("create their profile", StringComparison.OrdinalIgnoreCase)
               && !adminNotificationTemplate.Contains("Usr.tbUser", StringComparison.Ordinal)
               && !adminNotificationTemplate.Contains("domain user account", StringComparison.OrdinalIgnoreCase),
            "The registration notification is not a clear, user-facing route to registration review.");

        var identityEmailSender = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Mail", "IdentityEmailSender.cs"));
        Assert(identityEmailSender.Contains(
                "Arguments.Add(\"UsersUrl\", WebUtility.HtmlEncode(usersUrl))",
                StringComparison.Ordinal),
            "The Users-panel URL is not HTML encoded before template substitution.");

        var usersPanel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Pages", "Admin", "Manager", "Components", "UsersPanel.razor"));
        Assert(usersPanel.Contains(
                "settings.HasMailHost || PlatformEmailSender.IsConfigured",
                StringComparison.Ordinal)
               && usersPanel.Contains("Active email route configured", StringComparison.Ordinal),
            "Registration readiness does not recognise the platform email route.");
        Assert(usersPanel.Contains(".Select(item => new RegistrationRow", StringComparison.Ordinal)
               && usersPanel.Contains("EmailAddress = item.EmailAddress ?? string.Empty", StringComparison.Ordinal)
               && usersPanel.Contains("UserName = item.UserName ?? string.Empty", StringComparison.Ordinal),
            "The User panel does not coalesce nullable registration-view fields in its SQL projection.");

        var registrationModel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Models", "AspNet_UserRegistration.cs"));
        Assert(registrationModel.Contains("public string? EmailAddress", StringComparison.Ordinal)
               && registrationModel.Contains("public string? UserName", StringComparison.Ordinal),
            "Pending registrations cannot represent the nullable profile fields returned by the registration view.");

        var metadataOptions = new DbContextOptionsBuilder<NodeContext>()
            .UseSqlServer("Server=(local);Database=metadata-only;Integrated Security=True;TrustServerCertificate=True")
            .Options;
        using (var metadataContext = new NodeContext(metadataOptions))
        {
            var registrationEntity = metadataContext.Model.FindEntityType(typeof(AspNet_UserRegistration))
                ?? throw new InvalidOperationException("Registration metadata is missing.");
            Assert(registrationEntity.FindProperty(nameof(AspNet_UserRegistration.EmailAddress))?.IsNullable == true
                   && registrationEntity.FindProperty(nameof(AspNet_UserRegistration.UserName))?.IsNullable == true,
                "EF runtime metadata still treats pending-registration profile fields as required.");
        }

        var eventViewerPanel = File.ReadAllText(Path.Combine(
            root, "src", "TCWeb", "Pages", "Admin", "Manager", "Components", "EventViewerPanel.razor"));
        Assert(eventViewerPanel.Contains(
                "PlatformEmailSender.IsConfigured || await nodeSettings.MailHost() is not null",
                StringComparison.Ordinal),
            "Support readiness does not recognise the platform email route.");

        return assertions;
    }
}
