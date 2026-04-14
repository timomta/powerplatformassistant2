using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowerPlatformAssistant.Web.Data;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Guidance;
using PowerPlatformAssistant.Web.Services.Naming;
using PowerPlatformAssistant.Web.Services.Screenshots;
using PowerPlatformAssistant.Web.Services.Tenant;
using AppContextModel = PowerPlatformAssistant.Web.Models.AppContext;

namespace PowerPlatformAssistant.Web.Services.Chat;

public sealed class ConversationService(
    PowerPlatformAssistantDbContext dbContext,
    TenantContextService tenantContextService,
    TenantContextRefreshService tenantContextRefreshService,
    PromptCompositionService promptCompositionService,
    OnboardingService onboardingService,
    AuthoringFlowService authoringFlowService,
    AppRouteTransitionService appRouteTransitionService,
    NamingPreferenceService namingPreferenceService,
    ScreenshotIntakeService screenshotIntakeService,
    DebuggingGuidanceService debuggingGuidanceService,
    AssistantResponseService assistantResponseService,
    UntrustedInputGuard inputGuard,
    IOptions<GovernanceOptions> governanceOptions)
{
    public async Task<ChatConversationState> GetStateAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    public async Task<ChatConversationState> CompleteOnboardingAsync(ClaimsPrincipal user, CompleteOnboardingRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        var appliedOnboardingState = onboardingService.Apply(context.Session, request);
        var checklistTemplate = onboardingService.BuildChecklist(appliedOnboardingState, context.Conversation.ConversationId);

        var onboardingState = context.OnboardingState;
        if (onboardingState is null)
        {
            onboardingState = appliedOnboardingState;
            context.Session.OnboardingState = onboardingState;
            dbContext.OnboardingStates.Add(onboardingState);
        }
        else
        {
            onboardingState.ExperienceLevel = appliedOnboardingState.ExperienceLevel;
            onboardingState.FlowType = appliedOnboardingState.FlowType;
            onboardingState.AppContext = appliedOnboardingState.AppContext;
            onboardingState.ScopeAcknowledged = appliedOnboardingState.ScopeAcknowledged;
            onboardingState.TenantContextAcknowledged = appliedOnboardingState.TenantContextAcknowledged;
            onboardingState.IsCompleted = appliedOnboardingState.IsCompleted;
            onboardingState.UpdatedAt = appliedOnboardingState.UpdatedAt;
            onboardingState.CompletedAt = appliedOnboardingState.CompletedAt;
        }

        var checklist = context.ActiveChecklist;
        if (checklist is null)
        {
            checklist = checklistTemplate;
            dbContext.GuidanceChecklists.Add(checklist);
        }
        else
        {
            if (checklist.Steps.Count > 0)
            {
                dbContext.GuidanceChecklistSteps.RemoveRange(checklist.Steps);
                checklist.Steps.Clear();
            }

            checklist.ChecklistTitle = checklistTemplate.ChecklistTitle;
            checklist.ChecklistStatus = checklistTemplate.ChecklistStatus;
            checklist.Steps = checklistTemplate.Steps;
        }

        context.Session.OnboardingState = onboardingState;
        context.ActiveChecklist = checklist;
        context.Conversation.ActiveChecklist = checklist;

        context.Conversation.ActiveStoryContext = "guided-chat-onboarding";
        context.Conversation.LastConfirmedStep = checklist.Steps.OrderBy(step => step.StepOrder).FirstOrDefault()?.StepText ?? string.Empty;
        context.Conversation.UpdatedAt = DateTimeOffset.UtcNow;
        context.Session.LastActivityAt = DateTimeOffset.UtcNow;

        var onboardingTurn = new ConversationTurn
        {
            ConversationId = context.Conversation.ConversationId,
            SenderType = "system",
            HasChecklistOutput = true,
            MessageText = $"Onboarding completed for the {onboardingState.ExperienceLevel} {onboardingState.FlowType} path. Confirm checklist step 1 before requesting the next guided action."
        };

        context.Conversation.Turns.Add(onboardingTurn);
        await SaveAsync(cancellationToken);
        return BuildState(context.Session, context.Conversation, onboardingState, checklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    public async Task<ChatConversationState> UpdateAuthoringContextAsync(ClaimsPrincipal user, AuthoringContextRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        EnsureOnboardingComplete(context);

        var transition = appRouteTransitionService.Apply(context.AppContext, request, context.Conversation.ConversationId);
        var environmentContext = context.EnvironmentContext ?? new EnvironmentContext { ConversationId = context.Conversation.ConversationId };
        environmentContext.EnvironmentId = string.IsNullOrWhiteSpace(request.EnvironmentId) ? context.TenantContext.EnvironmentId : request.EnvironmentId.Trim();
        environmentContext.EnvironmentType = string.IsNullOrWhiteSpace(request.EnvironmentType) ? context.TenantContext.EnvironmentType : request.EnvironmentType.Trim();
        environmentContext.Region = string.IsNullOrWhiteSpace(request.Region) ? context.TenantContext.Region : request.Region.Trim();
        environmentContext.CapabilitySummary = request.CapabilitySummary.Trim();
        environmentContext.HasCreationCapabilityUncertainty = request.HasCreationCapabilityUncertainty;
        environmentContext.UpdatedAt = DateTimeOffset.UtcNow;

        if (context.AppContext is null)
        {
            dbContext.AppContexts.Add(transition.AppContext);
        }

        if (context.EnvironmentContext is null)
        {
            dbContext.EnvironmentContexts.Add(environmentContext);
        }

        context.AppContext = transition.AppContext;
        context.EnvironmentContext = environmentContext;
        context.Session.CurrentFlowType = transition.AppContext.FlowType;
        if (context.OnboardingState is not null)
        {
            context.OnboardingState.FlowType = transition.AppContext.FlowType;
            context.OnboardingState.AppContext = transition.AppContext.CurrentGoal;
        }

        var systemTurn = new ConversationTurn
        {
            ConversationId = context.Conversation.ConversationId,
            SenderType = "system",
            HasChecklistOutput = true,
            MessageText = authoringFlowService.BuildSystemMessage(transition.AppContext, environmentContext, context.NamingPreferences, transition.RouteChanged)
        };

        dbContext.ConversationTurns.Add(systemTurn);
        await SaveAsync(cancellationToken);
        await ReloadConversationArtifactsAsync(context, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    public async Task<ChatConversationState> UpdateNamingPreferencesAsync(ClaimsPrincipal user, NamingPreferenceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        EnsureOnboardingComplete(context);

        context.NamingPreferences = (await namingPreferenceService.UpsertAsync(context.Conversation.ConversationId, request, cancellationToken)).ToList();

        var namingSummary = context.NamingPreferences.Count == 0
            ? "Naming preferences cleared for the current conversation."
            : $"Naming preferences pinned for this conversation: {string.Join(", ", context.NamingPreferences.Select(preference => $"{preference.ArtifactType}={preference.PreferredName}"))}.";

        dbContext.ConversationTurns.Add(new ConversationTurn
        {
            ConversationId = context.Conversation.ConversationId,
            SenderType = "system",
            MessageText = namingSummary
        });

        await SaveAsync(cancellationToken);
        await ReloadConversationArtifactsAsync(context, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    public async Task<ChatConversationState> UpdateDebuggingContextAsync(ClaimsPrincipal user, DebuggingContextRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        EnsureOnboardingComplete(context);

        inputGuard.ValidateScreenshotMetadata(request.ScreenshotFileName, request.ScreenshotContentType, request.ScreenshotFileSize);
        var intakeResult = screenshotIntakeService.Create(context.Conversation.ConversationId, request);

        dbContext.ScreenshotAttachments.Add(intakeResult.ScreenshotAttachment);

        if (context.DataSourceContext is null)
        {
            dbContext.DataSourceContexts.Add(intakeResult.DataSourceContext);
        }
        else
        {
            context.DataSourceContext.DataSourceName = intakeResult.DataSourceContext.DataSourceName;
            context.DataSourceContext.DataSourceCategory = intakeResult.DataSourceContext.DataSourceCategory;
            context.DataSourceContext.ResolutionStatus = intakeResult.DataSourceContext.ResolutionStatus;
            context.DataSourceContext.ClarificationNotes = intakeResult.DataSourceContext.ClarificationNotes;
            context.DataSourceContext.LastUpdatedAt = intakeResult.DataSourceContext.LastUpdatedAt;
        }

        context.Session.CurrentFlowType = "debugging";
        if (context.OnboardingState is not null)
        {
            context.OnboardingState.FlowType = "debugging";
        }

        dbContext.ConversationTurns.Add(new ConversationTurn
        {
            ConversationId = context.Conversation.ConversationId,
            SenderType = "system",
            HasChecklistOutput = true,
            MessageText = debuggingGuidanceService.BuildSystemMessage(intakeResult.ScreenshotAttachment, context.DataSourceContext ?? intakeResult.DataSourceContext, context.TenantContext)
        });

        await SaveAsync(cancellationToken);
        await ReloadConversationArtifactsAsync(context, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    public async Task<ChatConversationState> SubmitMessageAsync(ClaimsPrincipal user, ChatMessageRequest request, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        if (context.OnboardingState?.IsCompleted != true)
        {
            throw new InputGuardException(new Dictionary<string, string[]>
            {
                ["onboarding"] = ["Complete onboarding before submitting assistant questions."]
            });
        }

        var guardedMessage = inputGuard.ValidateChatMessage(request.MessageText);
        inputGuard.ValidateScreenshotMetadata(request.ScreenshotFileName, request.ScreenshotContentType, request.ScreenshotFileSize);

        var refreshResult = tenantContextRefreshService.Evaluate(context.Session, context.TenantContext);
        var promptComposition = await promptCompositionService.GetCurrentAsync(cancellationToken);

        var userTurn = new ConversationTurn
        {
            ConversationId = context.Conversation.ConversationId,
            SenderType = "user",
            MessageText = guardedMessage.MessageText
        };

        var assistantTurn = assistantResponseService.Generate(
            context.Session,
            context.OnboardingState,
            context.TenantContext,
            promptComposition,
            context.ActiveChecklist,
            guardedMessage.MessageText,
            request.HasScreenshot(),
            refreshResult,
            context.AppContext,
            context.EnvironmentContext,
            context.NamingPreferences,
            context.Screenshots.OrderByDescending(screenshot => screenshot.UploadedAt).FirstOrDefault(),
            context.DataSourceContext,
            authoringFlowService.BuildConversationContext(context.AppContext, context.EnvironmentContext),
            debuggingGuidanceService.BuildConversationContext(context.Screenshots.OrderByDescending(screenshot => screenshot.UploadedAt).FirstOrDefault(), context.DataSourceContext));

        dbContext.ConversationTurns.AddRange(userTurn, assistantTurn);

        await SaveAsync(cancellationToken);
        await ReloadConversationArtifactsAsync(context, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.AppContext, context.EnvironmentContext, context.NamingPreferences, context.Screenshots, context.DataSourceContext, context.TenantContext);
    }

    private async Task<ConversationContext> GetOrCreateContextAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var tenantContext = await tenantContextService.ResolveAsync(user, cancellationToken);
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user is missing a subject identifier.");
        var session = await dbContext.UserSessions
            .Include(current => current.Conversations)
                .ThenInclude(conversation => conversation.Turns)
            .SingleOrDefaultAsync(current => current.UserId == userId, cancellationToken);

        if (session is null)
        {
            session = UserSession.FromPrincipal(user, tenantContext);
            var conversation = new Conversation
            {
                ConversationId = session.CurrentConversationId,
                SessionId = session.SessionId,
                ActiveStoryContext = "guided-chat-onboarding"
            };

            session.Conversations.Add(conversation);
            dbContext.UserSessions.Add(session);
            return new ConversationContext(session, conversation, null, null, null, null, [], [], null, tenantContext);
        }

        session.TenantId = tenantContext.TenantId;
        session.LastActivityAt = DateTimeOffset.UtcNow;

        var activeConversation = session.Conversations.SingleOrDefault(conversation => conversation.ConversationId == session.CurrentConversationId)
            ?? session.Conversations.OrderByDescending(conversation => conversation.UpdatedAt).First();

        var onboardingState = await dbContext.OnboardingStates
            .SingleOrDefaultAsync(state => state.SessionId == session.SessionId, cancellationToken);
        var activeChecklist = await dbContext.GuidanceChecklists
            .Include(checklist => checklist.Steps)
            .SingleOrDefaultAsync(checklist => checklist.ConversationId == activeConversation.ConversationId, cancellationToken);
        var appContext = await dbContext.AppContexts
            .SingleOrDefaultAsync(current => current.ConversationId == activeConversation.ConversationId, cancellationToken);
        var environmentContext = await dbContext.EnvironmentContexts
            .SingleOrDefaultAsync(current => current.ConversationId == activeConversation.ConversationId, cancellationToken);
        var namingPreferences = await dbContext.NamingPreferences
            .Where(current => current.ConversationId == activeConversation.ConversationId)
            .OrderBy(current => current.ArtifactType)
            .ToListAsync(cancellationToken);
        var screenshots = await dbContext.ScreenshotAttachments
            .Where(current => current.ConversationId == activeConversation.ConversationId)
            .OrderByDescending(current => current.UploadedAt)
            .ToListAsync(cancellationToken);
        var dataSourceContext = await dbContext.DataSourceContexts
            .SingleOrDefaultAsync(current => current.ConversationId == activeConversation.ConversationId, cancellationToken);

        session.CurrentConversationId = activeConversation.ConversationId;
        session.OnboardingState = onboardingState;
        activeConversation.ActiveChecklist = activeChecklist;

        return new ConversationContext(session, activeConversation, onboardingState, activeChecklist, appContext, environmentContext, namingPreferences, screenshots, dataSourceContext, tenantContext);
    }

    private ChatConversationState BuildState(UserSession session, Conversation conversation, OnboardingState? onboardingState, GuidanceChecklist? activeChecklist, AppContextModel? appContext, EnvironmentContext? environmentContext, IReadOnlyList<NamingPreference> namingPreferences, IReadOnlyList<ScreenshotAttachment> screenshots, DataSourceContext? dataSourceContext, TenantContextSnapshot tenantContext)
    {
        return new ChatConversationState(
            session,
            tenantContext,
            governanceOptions.Value,
            onboardingState,
            activeChecklist,
            appContext,
            environmentContext,
            namingPreferences,
            screenshots,
            dataSourceContext,
            conversation.Turns.OrderBy(turn => turn.CreatedAt).ToList());
    }

    private async Task ReloadConversationArtifactsAsync(ConversationContext context, CancellationToken cancellationToken)
    {
        context.Conversation.Turns = await dbContext.ConversationTurns
            .Where(turn => turn.ConversationId == context.Conversation.ConversationId)
            .OrderBy(turn => turn.CreatedAt)
            .ToListAsync(cancellationToken);
        context.ActiveChecklist = await dbContext.GuidanceChecklists
            .Include(checklist => checklist.Steps)
            .SingleOrDefaultAsync(checklist => checklist.ConversationId == context.Conversation.ConversationId, cancellationToken);
        context.Conversation.ActiveChecklist = context.ActiveChecklist;
        context.AppContext = await dbContext.AppContexts.SingleOrDefaultAsync(current => current.ConversationId == context.Conversation.ConversationId, cancellationToken);
        context.EnvironmentContext = await dbContext.EnvironmentContexts.SingleOrDefaultAsync(current => current.ConversationId == context.Conversation.ConversationId, cancellationToken);
        context.NamingPreferences = await dbContext.NamingPreferences
            .Where(current => current.ConversationId == context.Conversation.ConversationId)
            .OrderBy(current => current.ArtifactType)
            .ToListAsync(cancellationToken);
        context.Screenshots = await dbContext.ScreenshotAttachments
            .Where(current => current.ConversationId == context.Conversation.ConversationId)
            .OrderByDescending(current => current.UploadedAt)
            .ToListAsync(cancellationToken);
        context.DataSourceContext = await dbContext.DataSourceContexts
            .SingleOrDefaultAsync(current => current.ConversationId == context.Conversation.ConversationId, cancellationToken);
    }

    private static void EnsureOnboardingComplete(ConversationContext context)
    {
        if (context.OnboardingState?.IsCompleted != true)
        {
            throw new InputGuardException(new Dictionary<string, string[]>
            {
                ["onboarding"] = ["Complete onboarding before updating authoring, naming, or debugging context."]
            });
        }
    }

    private Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class ConversationContext(
        UserSession session,
        Conversation conversation,
        OnboardingState? onboardingState,
        GuidanceChecklist? activeChecklist,
        AppContextModel? appContext,
        EnvironmentContext? environmentContext,
        IReadOnlyList<NamingPreference> namingPreferences,
        IReadOnlyList<ScreenshotAttachment> screenshots,
        DataSourceContext? dataSourceContext,
        TenantContextSnapshot tenantContext)
    {
        public UserSession Session { get; } = session;

        public Conversation Conversation { get; } = conversation;

        public OnboardingState? OnboardingState { get; } = onboardingState;

        public GuidanceChecklist? ActiveChecklist { get; set; } = activeChecklist;

        public AppContextModel? AppContext { get; set; } = appContext;

        public EnvironmentContext? EnvironmentContext { get; set; } = environmentContext;

        public IReadOnlyList<NamingPreference> NamingPreferences { get; set; } = namingPreferences;

        public IReadOnlyList<ScreenshotAttachment> Screenshots { get; set; } = screenshots;

        public DataSourceContext? DataSourceContext { get; set; } = dataSourceContext;

        public TenantContextSnapshot TenantContext { get; } = tenantContext;
    }
}

public sealed class ChatMessageRequest
{
    public ChatMessageRequest()
    {
    }

    public ChatMessageRequest(string messageText, string? screenshotFileName, string? screenshotContentType, long? screenshotFileSize)
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

public sealed record ChatConversationState(
    UserSession Session,
    TenantContextSnapshot TenantContext,
    GovernanceOptions Governance,
    OnboardingState? Onboarding,
    GuidanceChecklist? ActiveChecklist,
    AppContextModel? AppContext,
    EnvironmentContext? EnvironmentContext,
    IReadOnlyList<NamingPreference> NamingPreferences,
    IReadOnlyList<ScreenshotAttachment> Screenshots,
    DataSourceContext? DataSourceContext,
    IReadOnlyList<ConversationTurn> Turns);