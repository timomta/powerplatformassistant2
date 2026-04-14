using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PowerPlatformAssistant.Web.Security;

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestHeader";

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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(Request.Headers["X-Ppa-Display-Name"].ToString()) ? userId : Request.Headers["X-Ppa-Display-Name"].ToString()),
            new("tenant_id", string.IsNullOrWhiteSpace(Request.Headers["X-Ppa-Tenant-Id"].ToString()) ? "tenant-test" : Request.Headers["X-Ppa-Tenant-Id"].ToString()),
            new("environment_id", string.IsNullOrWhiteSpace(Request.Headers["X-Ppa-Environment-Id"].ToString()) ? "environment-test" : Request.Headers["X-Ppa-Environment-Id"].ToString()),
            new("environment_type", string.IsNullOrWhiteSpace(Request.Headers["X-Ppa-Environment-Type"].ToString()) ? "sandbox" : Request.Headers["X-Ppa-Environment-Type"].ToString()),
            new("region", string.IsNullOrWhiteSpace(Request.Headers["X-Ppa-Region"].ToString()) ? "unknown" : Request.Headers["X-Ppa-Region"].ToString()),
            new("licensing_signals", Request.Headers["X-Ppa-Licensing"].ToString()),
            new("capability_notes", Request.Headers["X-Ppa-Capabilities"].ToString()),
            new("governance_policy_notes", Request.Headers["X-Ppa-Governance"].ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}