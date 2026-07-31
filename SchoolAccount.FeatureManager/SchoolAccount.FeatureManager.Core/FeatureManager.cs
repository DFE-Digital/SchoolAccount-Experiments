using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core;

internal sealed class FeatureManager(
    IEnumerable<IFeatureProvider> providers,
    IOptions<FeatureOptions> options,
    IServiceProvider services)
    : IFeatureManager
{
    private readonly IFeatureProvider[] _providers = providers as IFeatureProvider[] ?? providers.ToArray();
    private readonly FeatureOptions _options = options.Value;

    public async ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        var context = new FeatureContext(featureName);

        foreach (var provider in _providers)
        {
            var decision = await provider.EvaluateAsync(context, services, cancellationToken).ConfigureAwait(false);
            
            if (decision != FeatureDecision.Unknown)
            {
                return decision == FeatureDecision.Enabled;
            }
        }

        return _options.DefaultResponse;
    }
}