using PowerPlatformAssistant.Web.Models;
using AppContextModel = PowerPlatformAssistant.Web.Models.AppContext;

namespace PowerPlatformAssistant.Web.Services.Guidance;

public sealed class AppRouteTransitionService
{
    public AppRouteTransitionResult Apply(AppContextModel? appContext, AuthoringContextRequest request, Guid conversationId)
    {
        var normalizedFlowType = NormalizeFlowType(request.FlowType);
        var updatedContext = appContext ?? new AppContextModel { ConversationId = conversationId };
        var routeChanged = !string.Equals(updatedContext.FlowType, normalizedFlowType, StringComparison.OrdinalIgnoreCase);

        updatedContext.FlowType = normalizedFlowType;
        updatedContext.AppName = request.AppName.Trim();
        updatedContext.AppIdentifier = request.AppIdentifier.Trim();
        updatedContext.ScreenName = request.ScreenName.Trim();
        updatedContext.CurrentGoal = request.CurrentGoal.Trim();
        updatedContext.IsRouteConfirmed = request.IsRouteConfirmed;
        updatedContext.UpdatedAt = DateTimeOffset.UtcNow;

        var transitionMessage = routeChanged
            ? $"Route changed to {normalizedFlowType}. The assistant will now avoid leaking guidance from the previous route."
            : $"Route confirmed as {normalizedFlowType}. Guidance will stay on this authoring path until you change it.";

        return new AppRouteTransitionResult(updatedContext, routeChanged, transitionMessage);
    }

    private static string NormalizeFlowType(string flowType)
    {
        var normalized = flowType.Trim().ToLowerInvariant();
        return normalized is "new-app" or "existing-app" ? normalized : "new-app";
    }
}

public sealed record AppRouteTransitionResult(AppContextModel AppContext, bool RouteChanged, string TransitionMessage);