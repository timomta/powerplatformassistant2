using System.Security.Claims;
using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Services.Tenant;

public sealed class TenantContextService(IHttpContextAccessor httpContextAccessor)
{
    public Task<TenantContextSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        return ResolveAsync(user, cancellationToken);
    }

    public Task<TenantContextSnapshot> ResolveAsync(ClaimsPrincipal? user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = new TenantContextSnapshot
        {
            TenantId = user?.FindFirstValue("tenant_id") ?? string.Empty,
            EnvironmentId = user?.FindFirstValue("environment_id") ?? string.Empty,
            EnvironmentType = user?.FindFirstValue("environment_type") ?? string.Empty,
            Region = user?.FindFirstValue("region") ?? string.Empty,
            LicensingSignals = user?.FindFirstValue("licensing_signals") ?? string.Empty,
            CapabilityNotes = user?.FindFirstValue("capability_notes") ?? string.Empty,
            GovernancePolicyNotes = user?.FindFirstValue("governance_policy_notes") ?? string.Empty
        };

        return Task.FromResult(snapshot);
    }
}