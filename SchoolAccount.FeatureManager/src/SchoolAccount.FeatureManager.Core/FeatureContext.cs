using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core;

public sealed class FeatureContext(
    string featureName, 
    IServiceProvider services, 
    TargetingContext targeting)
{
    /// <summary>The feature being evaluated.</summary>
    public string FeatureName { get; } = featureName ?? throw new ArgumentNullException(nameof(featureName));

    /// <summary>
    /// The scope the evaluation is running in. In an ASP.NET Core request this is the
    /// request scope, so providers can resolve scoped services (DbContext, HttpContext, ...).
    /// </summary>
    public IServiceProvider Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>Who the evaluation is for. Never null; <see cref="TargetingContext.Empty"/> when anonymous.</summary>
    public TargetingContext Targeting { get; } = targeting ?? throw new ArgumentNullException(nameof(targeting));

    /// <summary>Scratch space for passing hints between providers within a single evaluation.</summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}