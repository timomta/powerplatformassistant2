using System.Text;

namespace PowerPlatformAssistant.Web.Prompts;

public sealed class PromptCompositionService(IWebHostEnvironment environment)
{
    private readonly IReadOnlyDictionary<string, string> _artifactPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["constitution"] = Path.Combine(".specify", "memory", "constitution.md"),
        ["design-spec"] = Path.Combine(".specify", "memory", "application-design-spec.md"),
        ["system-prompt"] = Path.Combine(".specify", "memory", "system-prompt.md")
    };

    public async Task<PromptComposition> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var artifacts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var artifact in _artifactPaths)
        {
            var fullPath = Path.Combine(environment.ContentRootPath, artifact.Value);
            artifacts[artifact.Key] = File.Exists(fullPath)
                ? await File.ReadAllTextAsync(fullPath, cancellationToken)
                : $"[missing artifact: {artifact.Value}]";
        }

        var combinedPrompt = new StringBuilder()
            .AppendLine("Constitution:")
            .AppendLine(artifacts["constitution"])
            .AppendLine()
            .AppendLine("Design Spec:")
            .AppendLine(artifacts["design-spec"])
            .AppendLine()
            .AppendLine("System Prompt:")
            .AppendLine(artifacts["system-prompt"])
            .ToString();

        return new PromptComposition(
            artifacts["constitution"],
            artifacts["design-spec"],
            artifacts["system-prompt"],
            combinedPrompt);
    }
}

public sealed record PromptComposition(
    string Constitution,
    string DesignSpec,
    string SystemPrompt,
    string CombinedPrompt);