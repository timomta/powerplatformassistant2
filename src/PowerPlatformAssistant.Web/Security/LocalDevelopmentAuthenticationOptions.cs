using Microsoft.AspNetCore.Authentication;

namespace PowerPlatformAssistant.Web.Security;

public sealed class LocalDevelopmentAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "LocalDevelopmentAuthentication";

    public bool Enabled { get; set; } = true;

    public string UserId { get; set; } = "local-dev-user";

    public string DisplayName { get; set; } = "Local Developer";

    public string TenantId { get; set; } = "tenant-local";

    public string EnvironmentId { get; set; } = "environment-local";

    public string EnvironmentType { get; set; } = "sandbox";

    public string Region { get; set; } = "united-states";

    public string LicensingSignals { get; set; } = string.Empty;

    public string CapabilityNotes { get; set; } = string.Empty;

    public string GovernancePolicyNotes { get; set; } = string.Empty;
}