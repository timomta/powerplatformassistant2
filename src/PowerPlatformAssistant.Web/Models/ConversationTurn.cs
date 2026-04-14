namespace PowerPlatformAssistant.Web.Models;

public sealed class ConversationTurn
{
    public Guid TurnId { get; init; } = Guid.NewGuid();

    public Guid ConversationId { get; init; }

    public string SenderType { get; init; } = "assistant";

    public string MessageText { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public bool HasClarifyingQuestion { get; init; }

    public bool HasChecklistOutput { get; init; }

    public bool HasScopeBoundaryMessage { get; init; }
}