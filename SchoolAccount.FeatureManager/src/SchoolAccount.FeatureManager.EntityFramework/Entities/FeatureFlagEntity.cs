using SchoolAccount.FeatureManager.Core;

namespace SchoolAccount.FeatureManager.EntityFramework.Entities;

public class FeatureFlagEntity
{
    public int Id { get; set; }

    /// <summary>Feature name. Should be unique; add a unique index in your model.</summary>
    public string Name { get; set; } = default!;

    /// <summary>The plain on/off value, used when <see cref="RuleJson"/> is null.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Optional serialized <see cref="FeatureRule"/>. When present, targeting is evaluated
    /// against it and <see cref="IsEnabled"/> is ignored on read.
    /// </summary>
    public string? RuleJson { get; set; }
}