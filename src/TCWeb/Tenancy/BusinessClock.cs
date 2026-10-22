using System;

namespace TradeControl.Web.Tenancy;

/// <summary>
/// Supplies the accounting clock for the resolved business. User culture may
/// format these values but must not determine them.
/// </summary>
public interface IBusinessClock
{
    string TimeZoneId { get; }
    DateTimeOffset UtcNow { get; }
    DateTimeOffset LocalNow { get; }
    DateOnly Today { get; }
    DateTime TodayDateTime { get; }
    DateTimeOffset ToLocal(DateTime utcValue);
    string GetZoneLabel(DateTimeOffset localValue);
}

public sealed class BusinessClock(ITenantContext tenantContext, TimeProvider timeProvider)
    : IBusinessClock
{
    private TenantDescriptor Tenant => tenantContext.RequiredTenant;

    public string TimeZoneId => Tenant.BusinessTimeZoneId;

    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    public DateTimeOffset LocalNow =>
        TimeZoneInfo.ConvertTime(UtcNow, Tenant.BusinessTimeZone);

    public DateOnly Today => DateOnly.FromDateTime(LocalNow.DateTime);

    public DateTime TodayDateTime => Today.ToDateTime(TimeOnly.MinValue);

    public DateTimeOffset ToLocal(DateTime utcValue)
    {
        var utc = utcValue.Kind switch
        {
            DateTimeKind.Utc => utcValue,
            DateTimeKind.Local => utcValue.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utcValue, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTime(new DateTimeOffset(utc), Tenant.BusinessTimeZone);
    }

    public string GetZoneLabel(DateTimeOffset localValue)
    {
        if (TimeZoneId.Equals("Europe/London", StringComparison.OrdinalIgnoreCase)
            || TimeZoneId.Equals("GMT Standard Time", StringComparison.OrdinalIgnoreCase))
        {
            return Tenant.BusinessTimeZone.IsDaylightSavingTime(localValue.DateTime) ? "BST" : "GMT";
        }

        return $"UTC{localValue:zzz}";
    }
}
