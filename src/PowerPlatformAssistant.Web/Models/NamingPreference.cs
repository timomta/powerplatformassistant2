namespace PowerPlatformAssistant.Web.Models;

public sealed class NamingPreference
{
    public Guid NamingPreferenceId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string ArtifactType { get; set; } = string.Empty;

    public string PreferredName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class NamingPreferenceUpdateRequest
{
    public string AppName { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string ControlName { get; set; } = string.Empty;

    public string VariableName { get; set; } = string.Empty;
}