using System.Text.Json;
using Azure;
using Azure.Data.AppConfiguration;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Azure.Providers;

/// <summary>
/// Reads feature flags from Azure App Configuration. Flags there are stored under the
/// well-known key prefix <c>.appconfig.featureflag/</c>. A missing flag defers to the
/// next provider; the Azure SDK dependency is confined to this package.
/// </summary>
public sealed class AzureAppConfigurationFeatureProvider : IFeatureProvider
{
    // Well-known prefix used by App Configuration for feature flags. Declared locally
    // so the provider does not depend on an exact SDK constant name.
    private const string FeatureFlagKeyPrefix = ".appconfig.featureflag/";

    private readonly ConfigurationClient _client;
    private readonly string? _label;

    public AzureAppConfigurationFeatureProvider(ConfigurationClient client, string? label = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _label = label;
    }

    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var key = FeatureFlagKeyPrefix + context.FeatureName;

        try
        {
            Response<ConfigurationSetting> response =
                await _client.GetConfigurationSettingAsync(key, _label, cancellationToken).ConfigureAwait(false);

            if (response.Value is FeatureFlagConfigurationSetting typed)
            {
                return typed.IsEnabled ? FeatureDecision.Enabled : FeatureDecision.Disabled;
            }

            var raw = response.Value?.Value;
            
            if (string.IsNullOrWhiteSpace(raw))
            {
                return FeatureDecision.Unknown;
            }

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("enabled", out var enabled) 
                && enabled.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return enabled.GetBoolean() ? FeatureDecision.Enabled : FeatureDecision.Disabled;
            }

            return FeatureDecision.Unknown;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return FeatureDecision.Unknown;
        }
    }
}