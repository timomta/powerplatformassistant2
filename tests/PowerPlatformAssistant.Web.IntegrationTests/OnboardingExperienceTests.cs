using System.Net;
using System.Net.Http.Json;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Services.Chat;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class OnboardingExperienceTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task OnboardingCompletion_PersistsChecklistAndSessionState()
    {
        const string userId = "onboarding-user";
        using var client = CreateAuthenticatedClient(userId);

        var onboardingResponse = await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "beginner",
            FlowType = "new-app",
            AppContext = "Expense approval app",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        Assert.Equal(HttpStatusCode.OK, onboardingResponse.StatusCode);

        var onboardingState = await onboardingResponse.Content.ReadFromJsonAsync<ChatConversationState>();

        Assert.NotNull(onboardingState);
        Assert.Equal("beginner", onboardingState.Onboarding?.ExperienceLevel);
        Assert.Equal("new-app", onboardingState.Onboarding?.FlowType);
        Assert.NotNull(onboardingState.ActiveChecklist);
        Assert.Equal(3, onboardingState.ActiveChecklist.Steps.Count);

        var stateResponse = await client.GetFromJsonAsync<ChatConversationState>("/api/chat/state");

        Assert.NotNull(stateResponse);
        Assert.Equal("beginner", stateResponse.Onboarding?.ExperienceLevel);
        Assert.Equal("Expense approval app", stateResponse.Onboarding?.AppContext);
        Assert.Single(stateResponse.Turns, turn => turn.SenderType == "system");
    }

    [Fact]
    public async Task GuidedMessage_PersistsTurnsAndReturnsScopedResponse()
    {
        const string userId = "message-user";
        using var client = CreateAuthenticatedClient(userId);

        await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "beginner",
            FlowType = "existing-app",
            AppContext = "Inspection app",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        var messageResponse = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
            "I need help with a Power Apps screen formula.",
            null,
            null,
            null));

        Assert.Equal(HttpStatusCode.OK, messageResponse.StatusCode);

        var messageState = await messageResponse.Content.ReadFromJsonAsync<ChatConversationState>();

        Assert.NotNull(messageState);
        Assert.True(messageState.Turns.Count >= 3);
        Assert.Contains(messageState.Turns, turn => turn.SenderType == "user");
        Assert.Contains(messageState.Turns, turn => turn.SenderType == "assistant" && turn.HasChecklistOutput);
        Assert.Contains("existing-app", messageState.Turns.Last().MessageText);

        var persistedState = await client.GetFromJsonAsync<ChatConversationState>("/api/chat/state");

        Assert.NotNull(persistedState);
        Assert.Equal(messageState.Turns.Count, persistedState.Turns.Count);
    }

    [Fact]
    public async Task GuidedMessage_RejectsSubmissionBeforeOnboarding()
    {
        using var client = CreateAuthenticatedClient("pre-onboarding-user");

        var response = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
            "Help me with Power Apps.",
            null,
            null,
            null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("Complete onboarding", payload);
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