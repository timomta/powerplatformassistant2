using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlatformAssistant.Web.Models;

public sealed class Conversation
{
    public Guid ConversationId { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Status { get; set; } = "active";

    public string ActiveStoryContext { get; set; } = "guided-chat-onboarding";

    public string LastConfirmedStep { get; set; } = string.Empty;

    public List<ConversationTurn> Turns { get; set; } = [];

    [NotMapped]
    public GuidanceChecklist? ActiveChecklist { get; set; }
}