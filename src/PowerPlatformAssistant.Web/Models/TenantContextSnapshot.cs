namespace PowerPlatformAssistant.Web.Models;

public sealed class TenantContextSnapshot
{
    public string TenantId { get; init; } = string.Empty;

    public string EnvironmentId { get; init; } = string.Empty;

    public string EnvironmentType { get; init; } = string.Empty;

    public string Region { get; init; } = string.Empty;

    public string LicensingSignals { get; init; } = string.Empty;

    public string CapabilityNotes { get; init; } = string.Empty;

    public string GovernancePolicyNotes { get; init; } = string.Empty;

    public bool IsResolved => !string.IsNullOrWhiteSpace(TenantId) && !string.IsNullOrWhiteSpace(EnvironmentId);
}