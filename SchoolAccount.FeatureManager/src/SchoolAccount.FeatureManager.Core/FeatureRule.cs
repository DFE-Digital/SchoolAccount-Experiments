namespace SchoolAccount.FeatureManager.Core;

/// <summary>
/// A targeting rule attached to a stored feature. A plain on/off flag is just the
/// degenerate case (all sets empty, only <see cref="Default"/> set), so simple flags
/// stay simple. Evaluated by <see cref="RuleEvaluator"/>.
/// </summary>
/// <remarks>
/// Note: the case-insensitive comparers below apply to rules built in code. A rule
/// round-tripped through JSON loses the custom comparer, so keep identifier casing
/// consistent (or configure your serializer) if you rely on it.
/// </remarks>
public sealed class FeatureRule
{
    /// <summary>Baseline when nothing else matches. <c>null</c> means "no opinion" (defer to next provider).</summary>
    public bool? Default { get; set; }

    public HashSet<string> IncludeUsers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludeUsers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> IncludeGroups { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Attribute conditions, e.g. region ∈ {EU, UK}. Any match enables.</summary>
    public List<AttributeMatch> IncludeWhen { get; set; } = new();

    /// <summary>Stable percentage rollout, 0–100, hashed on feature + user id.</summary>
    public double RolloutPercentage { get; set; }
}

/// <summary>One attribute condition: the named attribute must equal one of the values.</summary>
public sealed record AttributeMatch(string Key, HashSet<string> Values);
