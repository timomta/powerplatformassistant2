using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Services.Screenshots;

public sealed class ScreenshotIntakeService
{
    public ScreenshotIntakeResult Create(Guid conversationId, DebuggingContextRequest request)
    {
        var screenshot = new ScreenshotAttachment
        {
            ConversationId = conversationId,
            OriginalFileName = request.ScreenshotFileName.Trim(),
            ContentType = request.ScreenshotContentType.Trim(),
            FileSize = request.ScreenshotFileSize,
            StorageReference = $"debugging/{conversationId:N}/{Guid.NewGuid():N}/{request.ScreenshotFileName.Trim()}",
            ReviewStatus = string.IsNullOrWhiteSpace(request.VisibleIssueSummary) ? "pending" : "reviewed",
            VisibleIssueSummary = request.VisibleIssueSummary.Trim()
        };

        var dataSourceContext = new DataSourceContext
        {
            ConversationId = conversationId,
            DataSourceName = request.DataSourceName.Trim(),
            DataSourceCategory = request.DataSourceCategory.Trim(),
            ResolutionStatus = string.IsNullOrWhiteSpace(request.ResolutionStatus) ? "unknown" : request.ResolutionStatus.Trim().ToLowerInvariant(),
            ClarificationNotes = request.ClarificationNotes.Trim(),
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        return new ScreenshotIntakeResult(screenshot, dataSourceContext);
    }
}

public sealed record ScreenshotIntakeResult(ScreenshotAttachment ScreenshotAttachment, DataSourceContext DataSourceContext);