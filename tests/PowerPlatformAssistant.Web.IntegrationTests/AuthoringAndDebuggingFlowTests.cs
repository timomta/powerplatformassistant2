using System.Net;
using System.Net.Http.Json;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Services.Chat;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class AuthoringAndDebuggingFlowTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AuthoringContext_AndNamingPreferences_PersistAcrossStateAndResponses()
    {
        const string userId = "authoring-user";
        using var client = CreateAuthenticatedClient(userId);

        await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "intermediate",
            FlowType = "new-app",
            AppContext = "Inspection starter",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        var authoringResponse = await client.PostAsJsonAsync("/api/chat/authoring-context", new AuthoringContextRequest
        {
            FlowType = "new-app",
            AppName = "Inspection Hub",
            AppIdentifier = "inspection-hub",
            ScreenName = "scrInspectionHome",
            CurrentGoal = "Create the first inspection screen",
            IsRouteConfirmed = true,
            EnvironmentId = "env-001",
            EnvironmentType = "sandbox",
            Region = "united-states",
            CapabilitySummary = "Canvas creation is allowed",
            HasCreationCapabilityUncertainty = true
        });

        Assert.Equal(HttpStatusCode.OK, authoringResponse.StatusCode);
        var authoringState = await authoringResponse.Content.ReadFromJsonAsync<ChatConversationState>();
        Assert.NotNull(authoringState);
        Assert.Equal("Inspection Hub", authoringState.AppContext?.AppName);
        Assert.Equal("sandbox", authoringState.EnvironmentContext?.EnvironmentType);

        var namingResponse = await client.PostAsJsonAsync("/api/chat/naming-preferences", new NamingPreferenceUpdateRequest
        {
            AppName = "InspectionHub",
            ScreenName = "scrInspectionHome",
            ControlName = "galInspectionTasks",
            VariableName = "varSelectedInspection"
        });

        Assert.Equal(HttpStatusCode.OK, namingResponse.StatusCode);
        var namingState = await namingResponse.Content.ReadFromJsonAsync<ChatConversationState>();
        Assert.NotNull(namingState);
        Assert.Equal(4, namingState.NamingPreferences.Count);

        var messageResponse = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
            "Help me with a Power Apps gallery formula for this screen.",
            null,
            null,
            null));

        Assert.Equal(HttpStatusCode.OK, messageResponse.StatusCode);
        var messageState = await messageResponse.Content.ReadFromJsonAsync<ChatConversationState>();
        Assert.NotNull(messageState);
        Assert.Contains("Pinned names", messageState.Turns.Last().MessageText);
        Assert.Contains("new-app", messageState.Turns.Last().MessageText);

        var routeSwitchResponse = await client.PostAsJsonAsync("/api/chat/authoring-context", new AuthoringContextRequest
        {
            FlowType = "existing-app",
            AppName = "Inspection Hub",
            AppIdentifier = "inspection-hub",
            ScreenName = "scrInspectionHome",
            CurrentGoal = "Fix an existing gallery formula",
            IsRouteConfirmed = true,
            EnvironmentId = "env-001",
            EnvironmentType = "sandbox",
            Region = "united-states"
        });

        Assert.Equal(HttpStatusCode.OK, routeSwitchResponse.StatusCode);
        var routeState = await routeSwitchResponse.Content.ReadFromJsonAsync<ChatConversationState>();
        Assert.NotNull(routeState);
        Assert.Equal("existing-app", routeState.AppContext?.FlowType);
        Assert.Contains(routeState.Turns, turn => turn.MessageText.Contains("existing app", StringComparison.OrdinalIgnoreCase) || turn.MessageText.Contains("existing-app", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DebuggingContext_PersistsScreenshotMetadata_AndTriggersClarifyingGuidance()
    {
        const string userId = "debugging-user";
        using var client = CreateAuthenticatedClient(userId);

        await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "beginner",
            FlowType = "debugging",
            AppContext = "Issue with gallery data",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        var debuggingResponse = await client.PostAsJsonAsync("/api/chat/debugging-context", new DebuggingContextRequest
        {
            ScreenshotFileName = "gallery-issue.png",
            ScreenshotContentType = "image/png",
            ScreenshotFileSize = 4096,
            VisibleIssueSummary = "Gallery stays empty after selecting a site",
            DataSourceName = "Site Inspections",
            DataSourceCategory = "Dataverse",
            ResolutionStatus = "unknown",
            ClarificationNotes = "Need to confirm whether the table is already connected."
        });

        Assert.Equal(HttpStatusCode.OK, debuggingResponse.StatusCode);
        var debuggingState = await debuggingResponse.Content.ReadFromJsonAsync<ChatConversationState>();

        Assert.NotNull(debuggingState);
        Assert.Single(debuggingState.Screenshots);
        Assert.Equal("unknown", debuggingState.DataSourceContext?.ResolutionStatus);

        var messageResponse = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
            "I need help with a Power Apps debugging path for this gallery.",
            "gallery-issue.png",
            "image/png",
            4096));

        Assert.Equal(HttpStatusCode.OK, messageResponse.StatusCode);
        var messageState = await messageResponse.Content.ReadFromJsonAsync<ChatConversationState>();
        Assert.NotNull(messageState);
        Assert.True(messageState.Turns.Last().HasClarifyingQuestion);
        Assert.Contains("Debugging evidence", messageState.Turns.Last().MessageText);
    }

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Ppa-User-Id", userId);
        client.DefaultRequestHeaders.Add("X-Ppa-Display-Name", $"{userId}-display");
        client.DefaultRequestHeaders.Add("X-Ppa-Tenant-Id", "tenant-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Id", "env-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Type", "sandbox");
        client.DefaultRequestHeaders.Add("X-Ppa-Region", "united-states");
        return client;
    }
}