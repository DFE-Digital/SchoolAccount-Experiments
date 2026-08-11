namespace SchoolAccount.FeatureManager.Core;

/// <summary>
/// The persisted shape of a feature: a bool and/or a targeting rule. This is the data
/// that is the same for every caller, so it is the thing that is safe to cache.
/// </summary>
public sealed record StoredFeature(bool? Enabled, FeatureRule? Rule);