using System.Security.Claims;

namespace PowerPlatformAssistant.Web.Models;

public sealed class UserSession
{
    public Guid SessionId { get; init; } = Guid.NewGuid();

    public string UserId { get; init; } = string.Empty;

    public string TenantId { get; init; } = string.Empty;

    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;

    public string ExperienceLevel { get; init; } = "unknown";

    public string CurrentFlowType { get; set; } = "onboarding";

    public bool ScopeAcknowledged { get; set; }

    public Guid CurrentConversationId { get; init; } = Guid.NewGuid();

    public static UserSession FromPrincipal(ClaimsPrincipal principal, TenantContextSnapshot tenantContext)
    {
        return new UserSession
        {
            UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown-user",
            TenantId = tenantContext.TenantId,
            ScopeAcknowledged = true,
            LastActivityAt = DateTimeOffset.UtcNow
        };
    }
}