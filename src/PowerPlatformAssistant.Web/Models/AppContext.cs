namespace PowerPlatformAssistant.Web.Models;

public sealed class AppContext
{
    public Guid AppContextId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string FlowType { get; set; } = "new-app";

    public string AppName { get; set; } = string.Empty;

    public string AppIdentifier { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string CurrentGoal { get; set; } = string.Empty;

    public bool IsRouteConfirmed { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class EnvironmentContext
{
    public Guid EnvironmentContextId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string EnvironmentId { get; set; } = string.Empty;

    public string EnvironmentType { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string CapabilitySummary { get; set; } = string.Empty;

    public bool HasCreationCapabilityUncertainty { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AuthoringContextRequest
{
    public string FlowType { get; set; } = "new-app";

    public string AppName { get; set; } = string.Empty;

    public string AppIdentifier { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string CurrentGoal { get; set; } = string.Empty;

    public bool IsRouteConfirmed { get; set; }

    public string EnvironmentId { get; set; } = string.Empty;

    public string EnvironmentType { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string CapabilitySummary { get; set; } = string.Empty;

    public bool HasCreationCapabilityUncertainty { get; set; }
}