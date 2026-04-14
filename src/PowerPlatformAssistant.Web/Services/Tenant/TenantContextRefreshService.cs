using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Services.Tenant;

public sealed class TenantContextRefreshService
{
    public TenantContextRefreshResult Evaluate(UserSession session, TenantContextSnapshot tenantContext)
    {
        var tenantChanged = !string.IsNullOrWhiteSpace(session.TenantId)
            && !string.Equals(session.TenantId, tenantContext.TenantId, StringComparison.OrdinalIgnoreCase);
        var flowResetRequired = tenantChanged && session.ScopeAcknowledged;

        if (tenantChanged)
        {
            session.TenantId = tenantContext.TenantId;
            session.CurrentFlowType = "onboarding";
            session.ScopeAcknowledged = false;

            return new TenantContextRefreshResult(
                true,
                flowResetRequired,
                "The active tenant context changed since the prior session state, so I reset the flow to a safe onboarding boundary.");
        }

        return new TenantContextRefreshResult(false, false, "Tenant context remains aligned with the current session.");
    }
}

public sealed record TenantContextRefreshResult(bool ContextChanged, bool RequiresClarification, string SystemMessage);