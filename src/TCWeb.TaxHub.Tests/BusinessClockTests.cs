using System.Globalization;
using TradeControl.Web.Tenancy;

internal static class BusinessClockTests
{
    public static int Run()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var london = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

        var springClock = Clock("Europe/London", london,
            new DateTimeOffset(2026, 3, 29, 1, 30, 0, TimeSpan.Zero));
        Assert(springClock.LocalNow == new DateTimeOffset(2026, 3, 29, 2, 30, 0, TimeSpan.FromHours(1)),
            "The business clock did not apply the UK spring daylight-saving transition.");
        Assert(springClock.Today == new DateOnly(2026, 3, 29),
            "The business date was incorrect after the UK spring transition.");
        var eventTime = springClock.ToLocal(new DateTime(2026, 10, 22, 13, 31, 40, DateTimeKind.Unspecified));
        Assert(eventTime == new DateTimeOffset(2026, 10, 22, 14, 31, 40, TimeSpan.FromHours(1)),
            "A UTC event timestamp was not converted to the business time zone.");
        Assert(springClock.GetZoneLabel(eventTime) == "BST",
            "The event timestamp did not identify British Summer Time.");

        var autumnBefore = Clock("Europe/London", london,
            new DateTimeOffset(2026, 10, 25, 0, 30, 0, TimeSpan.Zero));
        var autumnAfter = Clock("Europe/London", london,
            new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero));
        Assert(autumnBefore.LocalNow.Offset == TimeSpan.FromHours(1) &&
               autumnAfter.LocalNow.Offset == TimeSpan.Zero,
            "The business clock did not apply the UK autumn daylight-saving transition.");
        Assert(autumnBefore.LocalNow.Hour == 1 && autumnAfter.LocalNow.Hour == 1,
            "The repeated UK autumn hour was not represented correctly.");

        var sydney = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
        var sydneyClock = Clock("Australia/Sydney", sydney,
            new DateTimeOffset(2026, 1, 1, 13, 30, 0, TimeSpan.Zero));
        Assert(sydneyClock.Today == new DateOnly(2026, 1, 2),
            "The business date followed the UTC date instead of the tenant time zone.");

        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert(springClock.Today == new DateOnly(2026, 3, 29),
                "The user's display culture changed the authoritative business date.");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        Assert(springClock.TimeZoneId == "Europe/London",
            "The configured IANA business time-zone identifier was not retained.");
        Assert(springClock.TodayDateTime.Kind == DateTimeKind.Unspecified,
            "A date-only database parameter was incorrectly tagged as server-local or UTC time.");

        return assertions;
    }

    private static BusinessClock Clock(string id, TimeZoneInfo zone, DateTimeOffset utcNow)
    {
        var context = new TenantContext();
        context.Bind(new TenantDescriptor(Guid.NewGuid(), "business.example.test", "Tenant",
            "Server=test", id, zone));
        return new BusinessClock(context, new FixedTimeProvider(utcNow));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
