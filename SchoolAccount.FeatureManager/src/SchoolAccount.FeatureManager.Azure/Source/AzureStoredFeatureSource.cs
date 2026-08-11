using System.Text.Json;
using Azure;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Azure.Source;

/// <summary>
/// Reads feature flags from Azure App Configuration (key prefix
/// <c>.appconfig.featureflag/</c>) as a <see cref="StoredFeature"/>. If the flag carries
/// a <c>Microsoft.Targeting</c> client filter, it is mapped to a <see cref="FeatureRule"/>
/// so App Config audiences flow through the same rule evaluator as EF rules.
/// </summary>
/// <remarks>
/// The targeting-filter mapping is best-effort and written against the documented JSON
/// shape; verify it against your App Config instance. On any parse issue it falls back
/// to the plain enabled/disabled value.
/// </remarks>
public sealed class AzureStoredFeatureSource(ConfigurationClient client, IOptions<AzureFeatureOptions> azureOptions)
    : IStoredFeatureSource
{
    private readonly ConfigurationClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly AzureFeatureOptions _options = azureOptions.Value ?? throw new ArgumentNullException(nameof(azureOptions));

    public async Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            Response<ConfigurationSetting> response = await _client
                .GetConfigurationSettingAsync(_options.DefaultKeyPrefix + featureName, _options.Group,
                    cancellationToken)
                .ConfigureAwait(false);

            var raw = response.Value?.Value;
            
            return !string.IsNullOrWhiteSpace(raw) 
                ? Parse(raw) 
                : null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null; // no such flag -> no opinion
        }
    }

    private static StoredFeature Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var enabled = root.TryGetProperty("enabled", out var e)
                      && (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False)
                      && e.GetBoolean();

        // enabled:false means the flag is off entirely, filters or not.
        if (!enabled)
        {
            return new StoredFeature(false, Rule: null);
        }

        var rule = TryMapTargeting(root);
        return rule is not null
            ? new StoredFeature(Enabled: null, Rule: rule) // targeted: only matching subjects get it
            : new StoredFeature(true, Rule: null); // enabled with no filter: on for everyone
    }

    private static FeatureRule? TryMapTargeting(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("conditions", out var conditions) ||
                !conditions.TryGetProperty("client_filters", out var filters) ||
                filters.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var filter in filters.EnumerateArray())
            {
                if (!filter.TryGetProperty("name", out var name) 
                    || !string.Equals(name.GetString(), "Microsoft.Targeting", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!filter.TryGetProperty("parameters", out var parameters) 
                    || !parameters.TryGetProperty("Audience", out var audience))
                {
                    return null;
                }

                var rule = new FeatureRule { Default = false };

                if (audience.TryGetProperty("Users", out var users) && users.ValueKind == JsonValueKind.Array)
                {
                    foreach (var u in users.EnumerateArray())
                    {
                        if (u.GetString() is { } s)
                        {
                            rule.IncludeUsers.Add(s);
                        }
                    }
                }

                if (audience.TryGetProperty("Groups", out var groups) && groups.ValueKind == JsonValueKind.Array)
                {
                    foreach (var g in groups.EnumerateArray())
                    {
                        if (g.TryGetProperty("Name", out var gn) && gn.GetString() is { } s)
                        {
                            rule.IncludeGroups.Add(s);
                        }
                    }
                }

                if (audience.TryGetProperty("Exclusion", out var exclusion) 
                    && exclusion.TryGetProperty("Users", out var exUsers) && exUsers.ValueKind == JsonValueKind.Array)
                {
                    foreach (var u in exUsers.EnumerateArray())
                    {
                        if (u.GetString() is { } s)
                        {
                            rule.ExcludeUsers.Add(s);
                        }
                    }
                }

                if (audience.TryGetProperty("DefaultRolloutPercentage", out var pct) 
                    && pct.ValueKind == JsonValueKind.Number)
                {
                    rule.RolloutPercentage = pct.GetDouble();
                }

                return rule;
            }

            return null;
        }
        catch (JsonException)
        {
            return null; // malformed targeting -> treat as a plain enabled flag
        }
    }
}
