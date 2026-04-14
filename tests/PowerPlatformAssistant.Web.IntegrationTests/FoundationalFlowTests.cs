using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class FoundationalFlowTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AuthenticatedStartup_LoadsProtectedHomePage()
    {
        using var client = CreateAuthenticatedClient("foundational-home-user");

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Power Platform Assistant", html);
        Assert.Contains("Complete Guided Onboarding", html);
    }

    [Fact]
    public async Task ChatState_RejectsAnonymousRequests()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/chat/state");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Ppa-User-Id", userId);
        client.DefaultRequestHeaders.Add("X-Ppa-Display-Name", "Foundational Tester");
        client.DefaultRequestHeaders.Add("X-Ppa-Tenant-Id", "tenant-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Id", "env-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Type", "sandbox");
        client.DefaultRequestHeaders.Add("X-Ppa-Region", "united-states");
        return client;
    }
}