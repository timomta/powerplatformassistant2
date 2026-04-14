using PowerPlatformAssistant.Web.Models;
using AppContextModel = PowerPlatformAssistant.Web.Models.AppContext;

namespace PowerPlatformAssistant.Web.Services.Guidance;

public sealed class AuthoringFlowService
{
    public string BuildSystemMessage(AppContextModel appContext, EnvironmentContext environmentContext, IReadOnlyList<NamingPreference> namingPreferences, bool routeChanged)
    {
        var routeLabel = appContext.FlowType == "new-app" ? "new app" : "existing app";
        var uncertaintyLine = environmentContext.HasCreationCapabilityUncertainty
            ? "Environment-specific creation options are uncertain, so validate the exact starter choices before committing to a build path."
            : "Environment capability is sufficiently clear to continue with the current route without a creation-capability clarifier.";
        var namingLine = namingPreferences.Count == 0
            ? "No naming preferences are pinned yet. Save app, screen, control, or variable names if you want the assistant to preserve them."
            : $"Pinned names: {string.Join(", ", namingPreferences.Select(preference => $"{preference.ArtifactType}={preference.PreferredName}"))}.";
        var routeLine = routeChanged
            ? "The route switch was applied without carrying over the prior branch's authoring assumptions."
            : "The active authoring route remains stable.";

        return $"Authoring context saved for the {routeLabel} path. Target app: {Safe(appContext.AppName)}. Goal: {Safe(appContext.CurrentGoal)}. {routeLine} Environment: {Safe(environmentContext.EnvironmentType)} / {Safe(environmentContext.Region)}. {uncertaintyLine} {namingLine}";
    }

    public string BuildConversationContext(AppContextModel? appContext, EnvironmentContext? environmentContext)
    {
        if (appContext is null || environmentContext is null)
        {
            return "Authoring route context is not yet pinned, so guidance stays conservative until the route and environment details are saved.";
        }

        var routeLabel = appContext.FlowType == "new-app" ? "new-app creation" : "existing-app modification";
        var uncertaintyLine = environmentContext.HasCreationCapabilityUncertainty
            ? "Creation capability is still uncertain in this environment, so I may ask a route clarifier instead of assuming a starter option."
            : "Environment capability has been pinned for this route.";

        return $"Active authoring route: {routeLabel}. Current goal: {Safe(appContext.CurrentGoal)}. App: {Safe(appContext.AppName)}. Screen: {Safe(appContext.ScreenName)}. {uncertaintyLine}";
    }

    private static string Safe(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "not specified" : value.Trim();
    }
}