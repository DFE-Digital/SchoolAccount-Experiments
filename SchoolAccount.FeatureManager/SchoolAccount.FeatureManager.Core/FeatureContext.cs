namespace SchoolAccount.FeatureManager.Core;

public sealed class FeatureContext(string featureName)
{
    public string FeatureName { get; } = featureName ?? throw new ArgumentNullException(nameof(featureName));

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}