namespace PowerPlatformAssistant.Web.Models;

public sealed class ScreenshotAttachment
{
    public Guid ScreenshotAttachmentId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string StorageReference { get; set; } = string.Empty;

    public string ReviewStatus { get; set; } = "pending";

    public string RejectionReason { get; set; } = string.Empty;

    public string VisibleIssueSummary { get; set; } = string.Empty;
}

public sealed class DataSourceContext
{
    public Guid DataSourceContextId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string DataSourceName { get; set; } = string.Empty;

    public string DataSourceCategory { get; set; } = string.Empty;

    public string ResolutionStatus { get; set; } = "unknown";

    public string ClarificationNotes { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class DebuggingContextRequest
{
    public string ScreenshotFileName { get; set; } = string.Empty;

    public string ScreenshotContentType { get; set; } = string.Empty;

    public long ScreenshotFileSize { get; set; }

    public string VisibleIssueSummary { get; set; } = string.Empty;

    public string DataSourceName { get; set; } = string.Empty;

    public string DataSourceCategory { get; set; } = string.Empty;

    public string ResolutionStatus { get; set; } = "unknown";

    public string ClarificationNotes { get; set; } = string.Empty;
}