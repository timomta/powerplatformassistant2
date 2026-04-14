using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PowerPlatformAssistant.Web.Components;
using PowerPlatformAssistant.Web.Data;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Prompts;
using PowerPlatformAssistant.Web.Security;
using PowerPlatformAssistant.Web.Services.Chat;
using PowerPlatformAssistant.Web.Services.Guidance;
using PowerPlatformAssistant.Web.Services.Naming;
using PowerPlatformAssistant.Web.Services.Screenshots;
using PowerPlatformAssistant.Web.Services.Tenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
            options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
        })
        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ =>
        {
        });
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = LocalDevelopmentAuthenticationHandler.SchemeName;
            options.DefaultChallengeScheme = LocalDevelopmentAuthenticationHandler.SchemeName;
        })
        .AddScheme<LocalDevelopmentAuthenticationOptions, LocalDevelopmentAuthenticationHandler>(LocalDevelopmentAuthenticationHandler.SchemeName, options =>
        {
            builder.Configuration.GetSection(LocalDevelopmentAuthenticationOptions.SectionName).Bind(options);
        });
}
else
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = NegotiateDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
        })
        .AddNegotiate();
}
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

if (builder.Environment.IsEnvironment("Testing"))
{
    var testDatabaseName = $"power-platform-assistant-tests-{Guid.NewGuid():N}";
    builder.Services.AddDbContext<PowerPlatformAssistantDbContext>(options =>
        options.UseInMemoryDatabase(testDatabaseName));
}
else
{
    var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(dataDirectory);
    var connectionString = string.IsNullOrWhiteSpace(configuredConnection)
        ? $"Data Source={Path.Combine(dataDirectory, "powerplatformassistant.db")}"
        : configuredConnection.Replace("App_Data", dataDirectory, StringComparison.OrdinalIgnoreCase);

    builder.Services.AddDbContext<PowerPlatformAssistantDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.Configure<GovernanceOptions>(builder.Configuration.GetSection(GovernanceOptions.SectionName));
builder.Services.Configure<LocalDevelopmentAuthenticationOptions>(builder.Configuration.GetSection(LocalDevelopmentAuthenticationOptions.SectionName));
builder.Services.AddScoped<TenantContextService>();
builder.Services.AddScoped<TenantContextRefreshService>();
builder.Services.AddScoped<PromptCompositionService>();
builder.Services.AddScoped<UntrustedInputGuard>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<AuthoringFlowService>();
builder.Services.AddScoped<AppRouteTransitionService>();
builder.Services.AddScoped<NamingPreferenceService>();
builder.Services.AddScoped<ScreenshotIntakeService>();
builder.Services.AddScoped<DebuggingGuidanceService>();
builder.Services.AddScoped<AssistantResponseService>();
builder.Services.AddScoped<ConversationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PowerPlatformAssistantDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

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
app.MapGet("/api/chat/state",
    async Task<Results<Ok<ChatConversationState>, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        var response = await conversationService.GetStateAsync(user, cancellationToken);
        return TypedResults.Ok(response);
    })
    .RequireAuthorization();

app.MapPost("/api/chat/onboarding",
    async Task<Results<Ok<ChatConversationState>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        CompleteOnboardingRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.CompleteOnboardingAsync(user, request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InputGuardException exception)
        {
            return TypedResults.ValidationProblem(exception.ToDictionary());
        }
    })
    .RequireAuthorization();

app.MapPost("/api/chat/messages",
    async Task<Results<Ok<ChatConversationState>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        ChatMessageRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.SubmitMessageAsync(user, request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InputGuardException exception)
        {
            return TypedResults.ValidationProblem(exception.ToDictionary());
        }
    })
    .RequireAuthorization();

app.MapPost("/api/chat/authoring-context",
    async Task<Results<Ok<ChatConversationState>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        AuthoringContextRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.UpdateAuthoringContextAsync(user, request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InputGuardException exception)
        {
            return TypedResults.ValidationProblem(exception.ToDictionary());
        }
    })
    .RequireAuthorization();

app.MapPost("/api/chat/naming-preferences",
    async Task<Results<Ok<ChatConversationState>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        NamingPreferenceUpdateRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.UpdateNamingPreferencesAsync(user, request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InputGuardException exception)
        {
            return TypedResults.ValidationProblem(exception.ToDictionary());
        }
    })
    .RequireAuthorization();

app.MapPost("/api/chat/debugging-context",
    async Task<Results<Ok<ChatConversationState>, ValidationProblem, UnauthorizedHttpResult>> (
        ClaimsPrincipal user,
        DebuggingContextRequest request,
        ConversationService conversationService,
        CancellationToken cancellationToken) =>
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var response = await conversationService.UpdateDebuggingContextAsync(user, request, cancellationToken);
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
