using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Azure.Stores;

public sealed class AzureFeatureStore(ConfigurationClient client, IOptions<AzureFeatureOptions> azureOptions)
    : IFeatureStore
{
    private readonly ConfigurationClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly AzureFeatureOptions _options = azureOptions?.Value ?? throw new ArgumentNullException(nameof(azureOptions));

    public async Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        var setting = new FeatureFlagConfigurationSetting(featureName, enabled);
        await _client.SetConfigurationSettingAsync(setting, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string featureName, CancellationToken cancellationToken = default)
    {
        await _client
            .DeleteConfigurationSettingAsync(_options.DefaultKeyPrefix + featureName, _options.Group, cancellationToken)
            .ConfigureAwait(false);
    }
}