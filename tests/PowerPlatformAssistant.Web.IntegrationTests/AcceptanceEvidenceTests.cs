using System.Net;
using System.Net.Http.Json;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Services.Chat;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class AcceptanceEvidenceTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task ScopeBoundary_RehearsalMeetsSc003Threshold()
    {
        var prompts = new[]
        {
            "Can you debug my Photoshop plugin?",
            "Write a Kubernetes deployment manifest for me.",
            "Can you design a Salesforce flow?",
            "How do I manage my home Wi-Fi router?",
            "Can you optimize this Excel macro outside Power Platform?",
            "Can Power Apps call a Photoshop layer directly in every tenant?",
            "Is every Power Automate premium connector always available everywhere?",
            "Can all Dataverse environments use the same unsupported connector set?",
            "Is Power Apps desktop publishing universal across every region?",
            "Can every Power Platform tenant use the same rollout features today?"
        };

        var passCount = 0;
        for (var index = 0; index < prompts.Length; index++)
        {
            var state = await SendScopedPromptAsync($"scope-evidence-{index}", prompts[index]);
            var assistantText = state.Turns.Last(turn => turn.SenderType == "assistant").MessageText;

            if (assistantText.Contains("Microsoft Power Platform guidance", StringComparison.OrdinalIgnoreCase)
                || assistantText.Contains("Tenant `tenant-001`", StringComparison.OrdinalIgnoreCase)
                || assistantText.Contains("environment `env-001`", StringComparison.OrdinalIgnoreCase))
            {
                passCount++;
            }
        }

        Console.WriteLine($"Acceptance rehearsal SC-003: {passCount}/{prompts.Length} passes");
        Assert.Equal(prompts.Length, passCount);
    }

    [Fact]
    public async Task TenantAwareGuidance_RehearsalMeetsSc004Threshold()
    {
        var prompts = new[]
        {
            "Power Apps licensing is unclear in this environment. What should I assume?",
            "The tenant rollout status is uncertain for this Power Platform feature.",
            "Power Automate region availability seems ambiguous in this tenant.",
            "The Power Apps environment governance policy might block creation options.",
            "Dataverse rollout timing is uncertain for this tenant.",
            "Power Apps licensing and region details are incomplete.",
            "The Power Apps environment type may change what features I can use.",
            "Power Apps tenant governance and licensing are both uncertain right now.",
            "The Power Platform rollout and environment context are still not confirmed.",
            "I need tenant-sensitive Power Platform guidance with uncertain licensing."
        };

        var passCount = 0;
        for (var index = 0; index < prompts.Length; index++)
        {
            var state = await SendScopedPromptAsync($"tenant-evidence-{index}", prompts[index]);
            var assistantTurn = state.Turns.Last(turn => turn.SenderType == "assistant");
            var isPass = assistantTurn.HasClarifyingQuestion
                || assistantTurn.MessageText.Contains("Tenant `tenant-001`", StringComparison.OrdinalIgnoreCase)
                || assistantTurn.MessageText.Contains("validate", StringComparison.OrdinalIgnoreCase)
                || assistantTurn.MessageText.Contains("confirm", StringComparison.OrdinalIgnoreCase);

            if (isPass)
            {
                passCount++;
            }
        }

        Console.WriteLine($"Acceptance rehearsal SC-004: {passCount}/{prompts.Length} passes");
        Assert.InRange(passCount, 9, prompts.Length);
    }

    [Fact]
    public async Task ScreenshotDebugging_RehearsalMeetsSc005Threshold()
    {
        var scenarios = Enumerable.Range(0, 10)
            .Select(index => new
            {
                UserId = $"debug-evidence-{index}",
                HasSummary = index < 5,
                Summary = index < 5 ? "Gallery is empty after selecting a site." : string.Empty,
                ExpectedPhrase = index < 5 ? "Debugging evidence" : "insufficient"
            })
            .ToArray();

        var passCount = 0;
        foreach (var scenario in scenarios)
        {
            using var client = CreateAuthenticatedClient(scenario.UserId);

            await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
            {
                ExperienceLevel = "beginner",
                FlowType = "debugging",
                AppContext = "Acceptance evidence debugging",
                ScopeAcknowledged = true,
                TenantContextAcknowledged = true
            });

            var debuggingResponse = await client.PostAsJsonAsync("/api/chat/debugging-context", new DebuggingContextRequest
            {
                ScreenshotFileName = $"issue-{scenario.UserId}.png",
                ScreenshotContentType = "image/png",
                ScreenshotFileSize = 4,
                ScreenshotContent = [1, 2, 3, 4],
                VisibleIssueSummary = scenario.Summary,
                DataSourceName = "Inspections",
                DataSourceCategory = "Dataverse",
                ResolutionStatus = "unknown",
                ClarificationNotes = "Need to confirm connector state."
            });

            Assert.Equal(HttpStatusCode.OK, debuggingResponse.StatusCode);

            var messageResponse = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(
                "I need help with a Power Apps debugging issue.",
                null,
                null,
                null));

            Assert.Equal(HttpStatusCode.OK, messageResponse.StatusCode);

            var state = await messageResponse.Content.ReadFromJsonAsync<ChatConversationState>();
            var assistantText = state!.Turns.Last(turn => turn.SenderType == "assistant").MessageText;
            var hasPassSignal = assistantText.Contains(scenario.ExpectedPhrase, StringComparison.OrdinalIgnoreCase)
                || assistantText.Contains("visible issue", StringComparison.OrdinalIgnoreCase)
                || assistantText.Contains("clarifying question", StringComparison.OrdinalIgnoreCase);

            if (hasPassSignal)
            {
                passCount++;
            }
        }

        Console.WriteLine($"Acceptance rehearsal SC-005: {passCount}/{scenarios.Length} passes");
        Assert.InRange(passCount, 9, scenarios.Length);
    }

    private async Task<ChatConversationState> SendScopedPromptAsync(string userId, string prompt)
    {
        using var client = CreateAuthenticatedClient(userId);
        await client.PostAsJsonAsync("/api/chat/onboarding", new CompleteOnboardingRequest
        {
            ExperienceLevel = "intermediate",
            FlowType = "existing-app",
            AppContext = "Acceptance rehearsal",
            ScopeAcknowledged = true,
            TenantContextAcknowledged = true
        });

        var response = await client.PostAsJsonAsync("/api/chat/messages", new ChatMessageRequest(prompt, null, null, null));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ChatConversationState>())!;
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