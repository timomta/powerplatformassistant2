using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using PowerPlatformAssistant.Web.Components;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Chat;
using PowerPlatformAssistant.Web.Services.Tenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(TenantHeaderAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TenantHeaderAuthenticationHandler>(TenantHeaderAuthenticationHandler.SchemeName, options =>
    {
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.Configure<GovernanceOptions>(builder.Configuration.GetSection(GovernanceOptions.SectionName));
builder.Services.AddScoped<TenantContextService>();
builder.Services.AddScoped<PromptCompositionService>();
builder.Services.AddScoped<UntrustedInputGuard>();
builder.Services.AddScoped<ConversationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapPost("/api/chat/preview",
    async Task<Results<Ok<ChatPreviewResponse>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        ChatPreviewRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.PreviewAsync(user, request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InputGuardException exception)
        {
            return TypedResults.ValidationProblem(exception.ToDictionary());
        }
    })
    .RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;

internal sealed class TenantHeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TenantHeader";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Ppa-User-Id", out var userIdValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValues.ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var displayName = Request.Headers["X-Ppa-Display-Name"].ToString();
        var tenantId = Request.Headers["X-Ppa-Tenant-Id"].ToString();
        var environmentId = Request.Headers["X-Ppa-Environment-Id"].ToString();
        var environmentType = Request.Headers["X-Ppa-Environment-Type"].ToString();
        var region = Request.Headers["X-Ppa-Region"].ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(displayName) ? userId : displayName),
            new("tenant_id", string.IsNullOrWhiteSpace(tenantId) ? "tenant-dev" : tenantId),
            new("environment_id", string.IsNullOrWhiteSpace(environmentId) ? "environment-dev" : environmentId),
            new("environment_type", string.IsNullOrWhiteSpace(environmentType) ? "sandbox" : environmentType),
            new("region", string.IsNullOrWhiteSpace(region) ? "unknown" : region),
            new("licensing_signals", Request.Headers["X-Ppa-Licensing"].ToString() ?? string.Empty),
            new("capability_notes", Request.Headers["X-Ppa-Capabilities"].ToString() ?? string.Empty),
            new("governance_policy_notes", Request.Headers["X-Ppa-Governance"].ToString() ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
