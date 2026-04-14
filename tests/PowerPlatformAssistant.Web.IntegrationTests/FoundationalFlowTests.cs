using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PowerPlatformAssistant.Web.Services.Chat;

namespace PowerPlatformAssistant.Web.IntegrationTests;

public class FoundationalFlowTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task AuthenticatedStartup_LoadsProtectedHomePage()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Power Platform Assistant", html);
        Assert.Contains("Secure Chat Shell", html);
    }

    [Fact]
    public async Task PreviewEndpoint_RejectsAnonymousRequests()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/chat/preview", new ChatPreviewRequest("power apps help", null, null, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PreviewEndpoint_EnforcesScopeBoundaries()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/chat/preview", new ChatPreviewRequest("Write a Python web scraper for me.", null, null, null));
        var payload = await response.Content.ReadFromJsonAsync<ChatPreviewResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.AssistantTurn.HasScopeBoundaryMessage);
        Assert.Contains("Microsoft Power Platform guidance", payload.AssistantTurn.MessageText);
    }

    [Fact]
    public async Task PreviewEndpoint_RejectsInvalidScreenshotMetadata()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/chat/preview", new ChatPreviewRequest(
            "Power Apps form issue",
            "issue.bmp",
            "image/bmp",
            1024));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("screenshotContentType", payload);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Ppa-User-Id", "user-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Display-Name", "Foundational Tester");
        client.DefaultRequestHeaders.Add("X-Ppa-Tenant-Id", "tenant-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Id", "env-001");
        client.DefaultRequestHeaders.Add("X-Ppa-Environment-Type", "sandbox");
        client.DefaultRequestHeaders.Add("X-Ppa-Region", "united-states");
        return client;
    }
}