using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Services.Tenant;

namespace PowerPlatformAssistant.Web.Services.Chat;

public sealed class AssistantResponseService
{
    private static readonly string[] PowerPlatformKeywords =
    [
        "power apps",
        "power automate",
        "power platform",
        "dataverse",
        "canvas",
        "model-driven",
        "copilot studio"
    ];

    private static readonly string[] TenantSensitiveKeywords =
    [
        "license",
        "licensing",
        "tenant",
        "environment",
        "region",
        "governance",
        "rollout"
    ];

    public ConversationTurn Generate(
        UserSession session,
        OnboardingState onboardingState,
        TenantContextSnapshot tenantContext,
        PromptComposition promptComposition,
        GuidanceChecklist? checklist,
        string messageText,
        bool hasScreenshot,
        TenantContextRefreshResult refreshResult)
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
                MessageText = "I can only help with Microsoft Power Platform guidance. Reframe the request in terms of Power Apps, Power Automate, Dataverse, or another supported Microsoft Power Platform capability."
            };
        }

        if (TenantSensitiveKeywords.Any(messageLower.Contains) && !tenantContext.IsResolved)
        {
            return new ConversationTurn
            {
                ConversationId = session.CurrentConversationId,
                SenderType = "assistant",
                HasClarifyingQuestion = true,
                HasUncertaintyMessage = true,
                MessageText = "Before I answer a tenant-sensitive question, confirm the tenant environment and any licensing or rollout constraint I should assume. I will not imply universal capability without that context."
            };
        }

        var screenshotLine = hasScreenshot
            ? "I will treat screenshot metadata as untrusted debugging context only and I will not claim anything the visible evidence does not support."
            : "No screenshot evidence was supplied, so the next step is grounded only in the text context you provided.";
        var refreshLine = refreshResult.RequiresClarification
            ? "Your tenant or environment context changed after onboarding, so validate that the current environment is still the intended target before acting on the next step."
            : refreshResult.SystemMessage;
        var checklistLines = checklist is null
            ? string.Empty
            : string.Join(Environment.NewLine, checklist.Steps.OrderBy(step => step.StepOrder).Select(step => $"{step.StepOrder}. {step.StepText}"));

        var responsePrefix = onboardingState.ExperienceLevel switch
        {
            "beginner" => "Beginner path: follow the next action exactly and confirm the result before continuing.",
            "intermediate" => "Intermediate path: validate the next action against the current environment before chaining more changes.",
            _ => "Advanced path: treat the next action as a bounded hypothesis against the current tenant context."
        };

        return new ConversationTurn
        {
            ConversationId = session.CurrentConversationId,
            SenderType = "assistant",
            HasChecklistOutput = checklist is not null,
            HasUncertaintyMessage = refreshResult.RequiresClarification,
            MessageText = $"{responsePrefix}{Environment.NewLine}{refreshLine}{Environment.NewLine}Current route: {onboardingState.FlowType}. App context: {SafeValue(onboardingState.AppContext)}.{Environment.NewLine}Tenant `{tenantContext.TenantId}` / environment `{tenantContext.EnvironmentId}` remain the active guidance boundary.{Environment.NewLine}{screenshotLine}{Environment.NewLine}Next step: describe the exact screen, control, or authoring goal you want to change so I can keep the guidance confirmable.{Environment.NewLine}{(string.IsNullOrWhiteSpace(checklistLines) ? string.Empty : $"Checklist:{Environment.NewLine}{checklistLines}{Environment.NewLine}")}Prompt artifacts loaded server-side ({promptComposition.CombinedPrompt.Length} characters)."
        };
    }

    private static string SafeValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "not yet specified" : value.Trim();
    }
}