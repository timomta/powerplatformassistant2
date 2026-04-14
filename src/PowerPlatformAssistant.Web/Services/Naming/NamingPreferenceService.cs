using Microsoft.EntityFrameworkCore;
using PowerPlatformAssistant.Web.Data;
using PowerPlatformAssistant.Web.Models;

namespace PowerPlatformAssistant.Web.Services.Naming;

public sealed class NamingPreferenceService(PowerPlatformAssistantDbContext dbContext)
{
    private static readonly (string ArtifactType, Func<NamingPreferenceUpdateRequest, string> Selector)[] Mapping =
    [
        ("app", request => request.AppName),
        ("screen", request => request.ScreenName),
        ("control", request => request.ControlName),
        ("variable", request => request.VariableName)
    ];

    public async Task<IReadOnlyList<NamingPreference>> UpsertAsync(Guid conversationId, NamingPreferenceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var existingPreferences = await dbContext.NamingPreferences
            .Where(preference => preference.ConversationId == conversationId)
            .ToListAsync(cancellationToken);

        foreach (var mapping in Mapping)
        {
            var preferredName = mapping.Selector(request).Trim();
            var existing = existingPreferences.SingleOrDefault(preference => preference.ArtifactType == mapping.ArtifactType);

            if (string.IsNullOrWhiteSpace(preferredName))
            {
                if (existing is not null)
                {
                    dbContext.NamingPreferences.Remove(existing);
                }

                continue;
            }

            if (existing is null)
            {
                existing = new NamingPreference
                {
                    ConversationId = conversationId,
                    ArtifactType = mapping.ArtifactType,
                    PreferredName = preferredName,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                dbContext.NamingPreferences.Add(existing);
                existingPreferences.Add(existing);
            }
            else
            {
                existing.PreferredName = preferredName;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existingPreferences
            .Where(preference => !string.IsNullOrWhiteSpace(preference.PreferredName))
            .OrderBy(preference => preference.ArtifactType)
            .ToList();
    }
}