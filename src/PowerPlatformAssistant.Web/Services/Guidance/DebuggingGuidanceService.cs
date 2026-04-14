using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Services.Guidance;

public sealed class DebuggingGuidanceService
{
    public string BuildSystemMessage(ScreenshotAttachment screenshotAttachment, DataSourceContext dataSourceContext, TenantContextSnapshot tenantContext)
    {
        var evidenceLine = string.IsNullOrWhiteSpace(screenshotAttachment.VisibleIssueSummary)
            ? "No visible issue summary was supplied, so the screenshot evidence is currently insufficient and still needs a user-readable problem statement."
            : $"Visible issue summary: {screenshotAttachment.VisibleIssueSummary}.";
        var dataSourceLine = dataSourceContext.ResolutionStatus is "unknown" or "planned"
            ? "The data-source state is unresolved, so the assistant will ask clarifying questions before assuming a connection path."
            : $"Data source `{Safe(dataSourceContext.DataSourceName)}` is currently marked as `{dataSourceContext.ResolutionStatus}`.";
        var storageLine = screenshotAttachment.HasStoredArtifact
            ? $"The screenshot artifact was stored server-side with retention {(screenshotAttachment.RetentionExpiresAt is null ? "pending policy configuration" : $"until {screenshotAttachment.RetentionExpiresAt:yyyy-MM-dd}")} and hash `{screenshotAttachment.Sha256Hash}`."
            : "Only screenshot metadata was saved for this debugging session.";

        return $"Debugging context saved for tenant `{tenantContext.TenantId}` in environment `{tenantContext.EnvironmentId}`. {evidenceLine} {dataSourceLine} {storageLine} Screenshot review remains limited to visible evidence and debugging scope only.";
    }

    public string BuildConversationContext(ScreenshotAttachment? latestScreenshot, DataSourceContext? dataSourceContext)
    {
        if (latestScreenshot is null)
        {
            return "No screenshot debugging evidence is attached to this conversation yet.";
        }

        var evidenceLine = string.IsNullOrWhiteSpace(latestScreenshot.VisibleIssueSummary)
            ? "Screenshot evidence is attached but remains insufficient because it still lacks a grounded visible issue summary."
            : $"Latest screenshot summary: {latestScreenshot.VisibleIssueSummary}.";
        var dataSourceLine = dataSourceContext is null
            ? "Data-source context has not been recorded yet."
            : dataSourceContext.ResolutionStatus is "unknown" or "planned"
                ? "Data-source context is unresolved, so a clarifying question is still required before dependent instructions."
                : $"Data-source state: {Safe(dataSourceContext.DataSourceName)} ({dataSourceContext.ResolutionStatus}).";
        var artifactLine = latestScreenshot.HasStoredArtifact
            ? "The latest screenshot artifact is stored server-side and can be reviewed within the debugging boundary."
            : "Only screenshot metadata is available for the latest debugging evidence.";

        return $"Debugging evidence: {evidenceLine} {dataSourceLine} {artifactLine}";
    }

    private static string Safe(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "not specified" : value.Trim();
    }
}