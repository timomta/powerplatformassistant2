using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Services.Chat;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class PerformanceEvidenceTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task OnboardingCompletion_StaysWithinLatencyBudget()
    {
        using var client = CreateAuthenticatedClient("performance-onboarding-user");
        var stopwatch = Stopwatch.StartNew();

        var response = await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "beginner",
            FlowType = "new-app",
            AppContext = "Performance evidence app",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        stopwatch.Stop();
        Console.WriteLine($"Performance evidence: onboarding completion = {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.InRange(stopwatch.Elapsed.TotalMilliseconds, 0, 3000);
    }

    [Fact]
    public async Task FirstAssistantResponse_StaysWithinLatencyBudget()
    {
        using var client = CreateAuthenticatedClient("performance-first-response-user");

        await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "intermediate",
            FlowType = "existing-app",
            AppContext = "Performance evidence app",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
            "I need help with a Power Apps gallery formula.",
            null,
            null,
            null));
        stopwatch.Stop();

        Console.WriteLine($"Performance evidence: first assistant response = {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.InRange(stopwatch.Elapsed.TotalMilliseconds, 0, 3000);
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