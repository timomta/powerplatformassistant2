using System.Security.Claims;
using Microsoft.Extensions.Options;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Tenant;

namespace PowerPlatformAssistant.Web.Services.Chat;

public sealed class ConversationService(
    TenantContextService tenantContextService,
    PromptCompositionService promptCompositionService,
    UntrustedInputGuard inputGuard,
    IOptions<GovernanceOptions> governanceOptions)
{
    private static readonly string[] PowerPlatformKeywords =
    [
        "power apps",
        "power automate",
        "power platform",
        "dataverse",
        "canvas app",
        "model-driven"
    ];

    private static readonly string[] TenantSensitiveKeywords =
    [
        "license",
        "licensing",
        "tenant",
        "environment",
        "region",
        "availability"
    ];

    public async Task<ChatPreviewResponse> PreviewAsync(ClaimsPrincipal user, ChatPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var guardedMessage = inputGuard.ValidateChatMessage(request.MessageText);
        inputGuard.ValidateScreenshotMetadata(request.ScreenshotFileName, request.ScreenshotContentType, request.ScreenshotFileSize);

        var tenantContext = await tenantContextService.ResolveAsync(user, cancellationToken);
        var promptComposition = await promptCompositionService.GetCurrentAsync(cancellationToken);
        var userSession = UserSession.FromPrincipal(user, tenantContext);

        var userTurn = new ConversationTurn
        {
            ConversationId = userSession.CurrentConversationId,
            SenderType = "user",
            MessageText = guardedMessage.MessageText
        };

        var assistantTurn = BuildAssistantTurn(userSession, tenantContext, promptComposition, guardedMessage.MessageText, request.HasScreenshot());

        return new ChatPreviewResponse(
            userSession,
            tenantContext,
            governanceOptions.Value,
            userTurn,
            assistantTurn);
    }

    private static ConversationTurn BuildAssistantTurn(
        UserSession session,
        TenantContextSnapshot tenantContext,
        PromptComposition promptComposition,
        string messageText,
        bool hasScreenshot)
    {
        var normalizedMessage = messageText.Trim();
        var messageLower = normalizedMessage.ToLowerInvariant();

        if (!PowerPlatformKeywords.Any(messageLower.Contains))
        {
            return new ConversationTurn
            {
                ConversationId = session.CurrentConversationId,
                SenderType = "assistant",
                HasScopeBoundaryMessage = true,
                MessageText = "I can only help with Microsoft Power Platform guidance. Please restate the question in terms of Power Apps, Power Automate, Dataverse, or another Microsoft Power Platform capability."
            };
        }

        if (TenantSensitiveKeywords.Any(messageLower.Contains) && !tenantContext.IsResolved)
        {
            return new ConversationTurn
            {
                ConversationId = session.CurrentConversationId,
                SenderType = "assistant",
                HasClarifyingQuestion = true,
                MessageText = "Before I give tenant-sensitive guidance, which tenant environment and licensing context should I assume? I need that context to avoid implying a universally available capability."
            };
        }

        var screenshotLine = hasScreenshot
            ? "I will treat any screenshot metadata as untrusted debugging evidence only and avoid certainty beyond what the visible context supports."
            : "No screenshot evidence was supplied, so the next step is based only on the stated tenant-aware text context.";

        return new ConversationTurn
        {
            ConversationId = session.CurrentConversationId,
            SenderType = "assistant",
            HasChecklistOutput = true,
            MessageText = $"Scope confirmed: Microsoft Power Platform guidance only. Tenant `{tenantContext.TenantId}` in environment `{tenantContext.EnvironmentId}` is the current server-side context. {screenshotLine} Next step: tell me the specific Power Apps screen, control, or data-source behavior you want to change so I can give a confirmable checklist. Prompt artifacts loaded: constitution, design spec, and system prompt ({promptComposition.CombinedPrompt.Length} characters total)."
        };
    }
}

public sealed class ChatPreviewRequest
{
    public ChatPreviewRequest()
    {
    }

    public ChatPreviewRequest(string messageText, string? screenshotFileName, string? screenshotContentType, long? screenshotFileSize)
    {
        MessageText = messageText;
        ScreenshotFileName = screenshotFileName;
        ScreenshotContentType = screenshotContentType;
        ScreenshotFileSize = screenshotFileSize;
    }

    public string MessageText { get; set; } = string.Empty;

    public string? ScreenshotFileName { get; set; }

    public string? ScreenshotContentType { get; set; }

    public long? ScreenshotFileSize { get; set; }

    public bool HasScreenshot()
    {
        return !string.IsNullOrWhiteSpace(ScreenshotFileName)
            || !string.IsNullOrWhiteSpace(ScreenshotContentType)
            || ScreenshotFileSize is > 0;
    }
}

public sealed record ChatPreviewResponse(
    UserSession Session,
    TenantContextSnapshot TenantContext,
    GovernanceOptions Governance,
    ConversationTurn UserTurn,
    ConversationTurn AssistantTurn);