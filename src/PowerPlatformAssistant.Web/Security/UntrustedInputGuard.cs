using System.Collections.ObjectModel;

namespace PowerPlatformAssistant.Web.Security;

public sealed class UntrustedInputGuard
{
    private static readonly HashSet<string> AllowedScreenshotContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    public const int MaxChatCharacters = 4000;
    public const long MaxScreenshotBytes = 10 * 1024 * 1024;

    public GuardedChatMessage ValidateChatMessage(string? messageText)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var normalized = messageText?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            errors["messageText"] = ["Message text is required."];
        }
        else if (normalized.Length > MaxChatCharacters)
        {
            errors["messageText"] = [$"Message text must be {MaxChatCharacters} characters or fewer."];
        }

        if (errors.Count > 0)
        {
            throw new InputGuardException(errors);
        }

        return new GuardedChatMessage(normalized!);
    }

    public void ValidateScreenshotMetadata(string? fileName, string? contentType, long? fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName) && string.IsNullOrWhiteSpace(contentType) && fileSize is null)
        {
            return;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors["screenshotFileName"] = ["Screenshot file name is required when screenshot metadata is supplied."];
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedScreenshotContentTypes.Contains(contentType))
        {
            errors["screenshotContentType"] = ["Screenshot content type must be PNG, JPEG, or WebP."];
        }

        if (fileSize is null || fileSize <= 0 || fileSize > MaxScreenshotBytes)
        {
            errors["screenshotFileSize"] = [$"Screenshot file size must be between 1 byte and {MaxScreenshotBytes} bytes."];
        }

        if (errors.Count > 0)
        {
            throw new InputGuardException(errors);
        }
    }
}

public sealed record GuardedChatMessage(string MessageText);

public sealed class InputGuardException(IReadOnlyDictionary<string, string[]> errors) : Exception("Input validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new ReadOnlyDictionary<string, string[]>(errors.ToDictionary(pair => pair.Key, pair => pair.Value));

    public Dictionary<string, string[]> ToDictionary()
    {
        return Errors.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }
}