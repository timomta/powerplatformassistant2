using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PowerPlatformAssistant.Web.Models;
using PowerPlatformAssistant.Web.Security;

namespace PowerPlatformAssistant.Web.Services.Screenshots;

public sealed class ScreenshotIntakeService(IWebHostEnvironment? environment = null, IOptions<GovernanceOptions>? governanceOptions = null)
{
    private readonly GovernanceOptions _governanceOptions = governanceOptions?.Value ?? new GovernanceOptions();

    public async Task<ScreenshotIntakeResult> CreateAsync(Guid conversationId, DebuggingContextRequest request, CancellationToken cancellationToken = default)
    {
        var uploadedAt = DateTimeOffset.UtcNow;
        var safeFileName = SanitizeFileName(request.ScreenshotFileName);
        var fileBytes = request.ScreenshotContent ?? [];
        var hasStoredArtifact = fileBytes.Length > 0;
        var storageReference = BuildStorageReference(conversationId, safeFileName, hasStoredArtifact);
        var sha256Hash = hasStoredArtifact ? Convert.ToHexString(SHA256.HashData(fileBytes)) : string.Empty;

        if (hasStoredArtifact)
        {
            var absolutePath = Path.Combine(GetStorageRoot(), storageReference.Replace('/', Path.DirectorySeparatorChar));
            var targetDirectory = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            await File.WriteAllBytesAsync(absolutePath, fileBytes, cancellationToken);
        }

        var screenshot = new ScreenshotAttachment
        {
            ConversationId = conversationId,
            UploadedAt = uploadedAt,
            OriginalFileName = safeFileName,
            ContentType = request.ScreenshotContentType.Trim(),
            FileSize = hasStoredArtifact ? fileBytes.Length : request.ScreenshotFileSize,
            StorageReference = storageReference,
            HasStoredArtifact = hasStoredArtifact,
            Sha256Hash = sha256Hash,
            StoredAt = uploadedAt,
            RetentionExpiresAt = _governanceOptions.ScreenshotRetentionDays > 0 ? uploadedAt.AddDays(_governanceOptions.ScreenshotRetentionDays) : null,
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
            LastUpdatedAt = uploadedAt
        };

        return new ScreenshotIntakeResult(screenshot, dataSourceContext);
    }

    private string GetStorageRoot()
    {
        var contentRoot = environment?.ContentRootPath ?? System.AppContext.BaseDirectory;
        return Path.Combine(contentRoot, "App_Data", "Screenshots");
    }

    private static string BuildStorageReference(Guid conversationId, string safeFileName, bool hasStoredArtifact)
    {
        var prefix = hasStoredArtifact ? "artifacts" : "metadata-only";
        return $"debugging/{conversationId:N}/{prefix}/{Guid.NewGuid():N}_{safeFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var safeName = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrWhiteSpace(safeName))
        {
            return "screenshot.bin";
        }

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidCharacter, '-');
        }

        return safeName;
    }
}

public sealed record ScreenshotIntakeResult(ScreenshotAttachment ScreenshotAttachment, DataSourceContext DataSourceContext);