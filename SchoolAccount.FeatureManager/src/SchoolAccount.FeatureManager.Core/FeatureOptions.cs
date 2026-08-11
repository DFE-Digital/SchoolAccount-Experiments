namespace SchoolAccount.FeatureManager.Core;

/// <summary>Global configuration for the feature pipeline.</summary>
public sealed class FeatureOptions
{
    /// <summary>
    /// Determines what a <see cref="FeatureDecision"/>.Unknown should return.
    /// </summary>
    public bool DefaultResponse { get; set; }
    
    /// <summary>
    /// Used for the group memory identifier against immutable prefixes.
    /// </summary>
    public string CacheStorePrefix { get; set; } = "safmc";
    
    /// <summary>
    /// Used for the feature memory identifier against the provider engine.
    /// </summary>
    public string CacheFeaturePrefix { get; set; } = "cfp";
    
    /// <summary>
    /// Determines if multiple sources are registered that you have to pick where the feature is defined; otherwise
    /// it'll loop through until it gets a result.
    /// </summary>
    public bool StrictSourceControl { get; set; } = true;
}