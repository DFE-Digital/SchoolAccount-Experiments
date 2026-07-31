using Microsoft.Extensions.DependencyInjection;

namespace SchoolAccount.FeatureManager.Core.DependencyInjection;

public sealed class FeatureFlagsBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
}