using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PowerPlatformAssistant.Web.Security;

internal sealed class LocalDevelopmentAuthenticationHandler(
    IOptionsMonitor<LocalDevelopmentAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<LocalDevelopmentAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "LocalDevelopment";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Options.Enabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Options.UserId),
            new(ClaimTypes.Name, Options.DisplayName),
            new("tenant_id", Options.TenantId),
            new("environment_id", Options.EnvironmentId),
            new("environment_type", Options.EnvironmentType),
            new("region", Options.Region),
            new("licensing_signals", Options.LicensingSignals),
            new("capability_notes", Options.CapabilityNotes),
            new("governance_policy_notes", Options.GovernancePolicyNotes)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}