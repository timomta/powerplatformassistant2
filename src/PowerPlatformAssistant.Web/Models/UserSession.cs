using System.Security.Claims;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlatformAssistant.Web.Models;

public sealed class UserSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;

    public string ExperienceLevel { get; set; } = "unknown";

    public string CurrentFlowType { get; set; } = "onboarding";

    public bool ScopeAcknowledged { get; set; }

    public Guid CurrentConversationId { get; set; } = Guid.NewGuid();

    public List<Conversation> Conversations { get; set; } = [];

    [NotMapped]
    public OnboardingState? OnboardingState { get; set; }

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