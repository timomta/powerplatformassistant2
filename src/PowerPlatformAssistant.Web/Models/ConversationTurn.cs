namespace PowerPlatformAssistant.Web.Models;

public sealed class ConversationTurn
{
    public Guid TurnId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string SenderType { get; set; } = "assistant";

    public string MessageText { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool HasClarifyingQuestion { get; set; }

    public bool HasChecklistOutput { get; set; }

    public bool HasScopeBoundaryMessage { get; set; }

    public bool HasUncertaintyMessage { get; set; }
}