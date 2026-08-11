using System.Security.Cryptography;
using System.Text;
using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core;

/// <summary>
/// Turns a <see cref="FeatureRule"/> plus a <see cref="TargetingContext"/> into a
/// <see cref="FeatureDecision"/>. Order is deliberate: exclusions beat includes,
/// explicit includes beat attribute/group matches, then percentage, then the default.
/// </summary>
public static class RuleEvaluator
{
    public static FeatureDecision Evaluate(string featureName, FeatureRule rule, TargetingContext target)
    {
        if (target.UserId is { } userId)
        {
            if (rule.ExcludeUsers.Contains(userId))
            {
                return FeatureDecision.Disabled; // exclusion wins outright
            }

            if (rule.IncludeUsers.Contains(userId))
            {
                return FeatureDecision.Enabled;
            }
        }

        if (rule.IncludeGroups.Count > 0 && rule.IncludeGroups.Overlaps(target.Groups))
        {
            return FeatureDecision.Enabled;
        }

        foreach (var match in rule.IncludeWhen)
            if (target.Attributes.TryGetValue(match.Key, out var value) && match.Values.Contains(value))
            {
                return FeatureDecision.Enabled;
            }

        if (rule.RolloutPercentage > 0 && target.UserId is { } id && InRollout(featureName, id, rule.RolloutPercentage))
        {
            return FeatureDecision.Enabled;
        }

        return rule.Default switch
        {
            true => FeatureDecision.Enabled,
            false => FeatureDecision.Disabled,
            null => FeatureDecision.Unknown // rule has no opinion for this subject -> stays composable
        };
    }

    /// <summary>
    /// Stable bucketing on a hash of feature + id. Mixing the feature name in means a
    /// user isn't stuck in the same bucket across every flag. Uses SHA-256, NOT
    /// <see cref="string.GetHashCode()"/>, which is randomized per process since .NET Core.
    /// </summary>
    private static bool InRollout(string featureName, string id, double percentage)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes($"{featureName}:{id}"), hash);
        var bucket = BitConverter.ToUInt32(hash) % 10_000 / 100.0; // 0.00–99.99
        return bucket < percentage;
    }
}
