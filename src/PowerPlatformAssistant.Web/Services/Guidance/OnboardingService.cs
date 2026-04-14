using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Security;

namespace PowerPlatformAssistant.Web.Services.Guidance;

public sealed class OnboardingService
{
    private static readonly HashSet<string> ValidExperienceLevels = ["beginner", "intermediate", "advanced"];
    private static readonly HashSet<string> ValidFlowTypes = ["new-app", "existing-app", "debugging"];

    public OnboardingState Apply(UserSession session, CompleteOnboardingRequest request)
    {
        var experienceLevel = NormalizeRequiredValue(request.ExperienceLevel, ValidExperienceLevels, nameof(request.ExperienceLevel));
        var flowType = NormalizeRequiredValue(request.FlowType, ValidFlowTypes, nameof(request.FlowType));

        if (!request.ScopeAcknowledged || !request.TenantContextAcknowledged)
        {
            throw new InputGuardException(new Dictionary<string, string[]>
            {
                ["onboarding"] = ["Scope and tenant context must be acknowledged before onboarding can complete."]
            });
        }

        session.ExperienceLevel = experienceLevel;
        session.CurrentFlowType = flowType;
        session.ScopeAcknowledged = true;
        session.LastActivityAt = DateTimeOffset.UtcNow;

        return new OnboardingState
        {
            SessionId = session.SessionId,
            ExperienceLevel = experienceLevel,
            FlowType = flowType,
            AppContext = request.AppContext.Trim(),
            ScopeAcknowledged = request.ScopeAcknowledged,
            TenantContextAcknowledged = request.TenantContextAcknowledged,
            IsCompleted = true,
            UpdatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    public GuidanceChecklist BuildChecklist(OnboardingState onboardingState, Guid conversationId)
    {
        var flowLabel = onboardingState.FlowType switch
        {
            "new-app" => "new app",
            "existing-app" => "existing app",
            _ => "debugging"
        };

        string[] steps = onboardingState.ExperienceLevel switch
        {
            "beginner" =>
            [
                $"Confirm the tenant environment and the exact {flowLabel} target before changing anything.",
                "State the one outcome you want from this step so the assistant can stay incremental.",
                "Validate the next step in the studio before asking for the follow-up step."
            ],
            "intermediate" =>
            [
                $"Confirm the current {flowLabel} path and the environment-specific constraint that matters most.",
                "Apply one change at a time and report the result back before moving to the next decision.",
                "Flag any tenant-specific uncertainty instead of assuming a broadly available capability."
            ],
            _ =>
            [
                $"State the exact {flowLabel} objective, current blocker, and known tenant constraint.",
                "Validate whether the next action depends on rollout, licensing, or environment configuration.",
                "Use the response as a narrow next-step plan rather than a speculative end-to-end redesign."
            ]
        };

        return new GuidanceChecklist
        {
            ConversationId = conversationId,
            ChecklistTitle = $"{ToDisplay(onboardingState.ExperienceLevel)} {ToDisplay(onboardingState.FlowType)} checklist",
            ChecklistStatus = "active",
            Steps = steps.Select((stepText, index) => new GuidanceChecklistStep
            {
                StepOrder = index + 1,
                StepText = stepText,
                ConfirmationStatus = "pending"
            }).ToList()
        };
    }

    private static string NormalizeRequiredValue(string value, IReadOnlySet<string> validValues, string fieldName)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !validValues.Contains(normalized))
        {
            throw new InputGuardException(new Dictionary<string, string[]>
            {
                [fieldName] = [$"{fieldName} must be one of: {string.Join(", ", validValues)}."]
            });
        }

        return normalized;
    }

    private static string ToDisplay(string value)
    {
        return string.Join(' ', value
            .Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}