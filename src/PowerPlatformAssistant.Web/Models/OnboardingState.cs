namespace PowerPlatformAssistant.Web.Models;

public sealed class OnboardingState
{
    public Guid OnboardingStateId { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    public string ExperienceLevel { get; set; } = "unknown";

    public string FlowType { get; set; } = "new-app";

    public string AppContext { get; set; } = string.Empty;

    public bool ScopeAcknowledged { get; set; }

    public bool TenantContextAcknowledged { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class GuidanceChecklist
{
    public Guid ChecklistId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string ChecklistTitle { get; set; } = string.Empty;

    public string ChecklistStatus { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<GuidanceChecklistStep> Steps { get; set; } = [];
}

public sealed class GuidanceChecklistStep
{
    public Guid StepId { get; set; } = Guid.NewGuid();

    public Guid ChecklistId { get; set; }

    public int StepOrder { get; set; }

    public string StepText { get; set; } = string.Empty;

    public string ConfirmationStatus { get; set; } = "pending";

    public DateTimeOffset? ConfirmedAt { get; set; }
}

public sealed class CompleteOnboardingRequest
{
    public string ExperienceLevel { get; set; } = string.Empty;

    public string FlowType { get; set; } = string.Empty;

    public string AppContext { get; set; } = string.Empty;

    public bool ScopeAcknowledged { get; set; }

    public bool TenantContextAcknowledged { get; set; }
}