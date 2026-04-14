using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Services.Tenant;
using AppContextModel = PowerPlatformAssistant.Web.Models.AppContext;

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
        TenantContextRefreshResult refreshResult,
        AppContextModel? appContext,
        EnvironmentContext? environmentContext,
        IReadOnlyList<NamingPreference> namingPreferences,
        ScreenshotAttachment? latestScreenshot,
        DataSourceContext? dataSourceContext,
        string authoringContextLine,
        string debuggingContextLine)
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

        var routeLabel = appContext?.FlowType ?? onboardingState.FlowType;
        var namingLine = namingPreferences.Count == 0
            ? "No naming preferences are currently pinned."
            : $"Pinned names: {string.Join(", ", namingPreferences.Select(preference => $"{preference.ArtifactType}={preference.PreferredName}"))}.";
        var unresolvedDebugging = routeLabel == "debugging" && dataSourceContext is not null && dataSourceContext.ResolutionStatus is "unknown" or "planned";
        var capabilityUncertainty = routeLabel == "new-app" && environmentContext?.HasCreationCapabilityUncertainty == true;
        var routeSpecificNextStep = routeLabel switch
        {
            "new-app" => "confirm the starter pattern, target screen structure, and first data dependency before creating additional surfaces.",
            "existing-app" => "identify the exact existing screen, control, or formula behavior to change before making the next edit.",
            "debugging" => latestScreenshot is null
                ? "attach a screenshot or give a visible issue summary before I narrow the debugging path."
                : "confirm the visible issue and the current data-source state before I suggest a root-cause-specific next action.",
            _ => "describe the exact Power Platform target you want to change so I can keep the guidance confirmable."
        };

        return new ConversationTurn
        {
            ConversationId = session.CurrentConversationId,
            SenderType = "assistant",
            HasChecklistOutput = checklist is not null,
            HasClarifyingQuestion = unresolvedDebugging || capabilityUncertainty,
            HasUncertaintyMessage = refreshResult.RequiresClarification || unresolvedDebugging || capabilityUncertainty,
            MessageText = $"{responsePrefix}{Environment.NewLine}{refreshLine}{Environment.NewLine}Current route: {routeLabel}. App context: {SafeValue(onboardingState.AppContext)}.{Environment.NewLine}{authoringContextLine}{Environment.NewLine}{debuggingContextLine}{Environment.NewLine}{namingLine}{Environment.NewLine}Tenant `{tenantContext.TenantId}` / environment `{tenantContext.EnvironmentId}` remain the active guidance boundary.{Environment.NewLine}{screenshotLine}{Environment.NewLine}Next step: {routeSpecificNextStep}{Environment.NewLine}{(string.IsNullOrWhiteSpace(checklistLines) ? string.Empty : $"Checklist:{Environment.NewLine}{checklistLines}{Environment.NewLine}")}Prompt artifacts loaded server-side ({promptComposition.CombinedPrompt.Length} characters)."
        };
    }

    private static string SafeValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "not yet specified" : value.Trim();
    }
}