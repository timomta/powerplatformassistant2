using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowerPlatformAssistant.Web.Data;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Guidance;
using PowerPlatformAssistant.Web.Services.Tenant;

namespace PowerPlatformAssistant.Web.Services.Chat;

public sealed class ConversationService(
    PowerPlatformAssistantDbContext dbContext,
    TenantContextService tenantContextService,
    TenantContextRefreshService tenantContextRefreshService,
    PromptCompositionService promptCompositionService,
    OnboardingService onboardingService,
    AssistantResponseService assistantResponseService,
    UntrustedInputGuard inputGuard,
    IOptions<GovernanceOptions> governanceOptions)
{
    public async Task<ChatConversationState> GetStateAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var context = await GetOrCreateContextAsync(user, cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.TenantContext);
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
        return BuildState(context.Session, context.Conversation, onboardingState, checklist, context.TenantContext);
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
            refreshResult);

        dbContext.ConversationTurns.AddRange(userTurn, assistantTurn);

        await SaveAsync(cancellationToken);
        context.Conversation.Turns = await dbContext.ConversationTurns
            .Where(turn => turn.ConversationId == context.Conversation.ConversationId)
            .OrderBy(turn => turn.CreatedAt)
            .ToListAsync(cancellationToken);
        return BuildState(context.Session, context.Conversation, context.OnboardingState, context.ActiveChecklist, context.TenantContext);
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
            return new ConversationContext(session, conversation, null, null, tenantContext);
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

        session.CurrentConversationId = activeConversation.ConversationId;
        session.OnboardingState = onboardingState;
        activeConversation.ActiveChecklist = activeChecklist;

        return new ConversationContext(session, activeConversation, onboardingState, activeChecklist, tenantContext);
    }

    private ChatConversationState BuildState(UserSession session, Conversation conversation, OnboardingState? onboardingState, GuidanceChecklist? activeChecklist, TenantContextSnapshot tenantContext)
    {
        return new ChatConversationState(
            session,
            tenantContext,
            governanceOptions.Value,
            onboardingState,
            activeChecklist,
            conversation.Turns.OrderBy(turn => turn.CreatedAt).ToList());
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
        TenantContextSnapshot tenantContext)
    {
        public UserSession Session { get; } = session;

        public Conversation Conversation { get; } = conversation;

        public OnboardingState? OnboardingState { get; } = onboardingState;

        public GuidanceChecklist? ActiveChecklist { get; set; } = activeChecklist;

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
    IReadOnlyList<ConversationTurn> Turns);