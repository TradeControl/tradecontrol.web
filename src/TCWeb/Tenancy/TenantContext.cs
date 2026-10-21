using System;
using System.Threading;

namespace TradeControl.Web.Tenancy;

public interface ITenantContext
{
    bool IsResolved { get; }
    TenantDescriptor RequiredTenant { get; }
    void Bind(TenantDescriptor tenant);
}

public sealed class TenantAmbientContext
{
    private readonly AsyncLocal<TenantDescriptor?> _current = new();
    public TenantDescriptor? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

public sealed class TenantContext : ITenantContext, IDisposable
{
    private readonly TenantAmbientContext _ambient;
    private TenantDescriptor? _tenant;
    private bool _ownsAmbientBinding;

    public TenantContext() : this(new TenantAmbientContext()) { }

    public TenantContext(TenantAmbientContext ambient)
    {
        _ambient = ambient;
        _tenant = ambient.Current;
    }

    public bool IsResolved => _tenant is not null;
    public TenantDescriptor RequiredTenant => _tenant
        ?? throw new InvalidOperationException("A tenant has not been resolved for this scope.");

    public void Bind(TenantDescriptor tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        if (_tenant is not null)
        {
            if (_tenant.TenantId == tenant.TenantId)
                return;

            throw new InvalidOperationException("The tenant is immutable for the lifetime of a scope.");
        }
        _tenant = tenant;
        _ambient.Current = tenant;
        _ownsAmbientBinding = true;
    }

    public void Dispose()
    {
        if (_ownsAmbientBinding && ReferenceEquals(_ambient.Current, _tenant))
            _ambient.Current = null;
    }
}
