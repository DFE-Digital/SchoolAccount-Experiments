using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Context;

/// <summary>
/// The subject an evaluation is reasoned about: who is asking and what we know about
/// them. Populated per evaluation by an <see cref="ITargetingContextAccessor"/>.
/// </summary>
public sealed class TargetingContext
{
    /// <summary>An empty context (anonymous, no attributes).</summary>
    public static TargetingContext Empty { get; } = new();

    /// <summary>Stable identifier for the subject (user id, tenant id, device id, ...).</summary>
    public string? UserId { get; init; }

    /// <summary>Groups/roles the subject belongs to. Matched against <see cref="FeatureRule.IncludeGroups"/>.</summary>
    public IReadOnlyCollection<string> Groups { get; init; } = Array.Empty<string>();

    /// <summary>Free-form attributes, e.g. ["region"] = "EU", ["plan"] = "premium".</summary>
    public IReadOnlyDictionary<string, string> Attributes { get; init; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
