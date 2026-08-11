using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Tests;

public class RuleEvaluatorTests
{
    [Fact]
    public void Exclusion_BeatsInclusion()
    {
        var rule = new FeatureRule { Default = false };
        rule.IncludeUsers.Add("alice");
        rule.ExcludeUsers.Add("alice");

        var result = RuleEvaluator.Evaluate("F", rule, new TargetingContext { UserId = "alice" });

        Assert.Equal(FeatureDecision.Disabled, result);
    }

    [Fact]
    public void IncludeUser_EnablesOnlyThatUser()
    {
        var rule = new FeatureRule { Default = false };
        rule.IncludeUsers.Add("alice");

        Assert.Equal(FeatureDecision.Enabled, RuleEvaluator.Evaluate("F", rule, new TargetingContext { UserId = "alice" }));
        Assert.Equal(FeatureDecision.Disabled, RuleEvaluator.Evaluate("F", rule, new TargetingContext { UserId = "bob" }));
    }

    [Fact]
    public void IncludeGroup_EnablesMembers()
    {
        var rule = new FeatureRule { Default = false };
        rule.IncludeGroups.Add("beta-testers");

        var inGroup = new TargetingContext { UserId = "x", Groups = new[] { "beta-testers" } };
        var notInGroup = new TargetingContext { UserId = "y", Groups = new[] { "other" } };

        Assert.Equal(FeatureDecision.Enabled, RuleEvaluator.Evaluate("F", rule, inGroup));
        Assert.Equal(FeatureDecision.Disabled, RuleEvaluator.Evaluate("F", rule, notInGroup));
    }

    [Fact]
    public void AttributeMatch_EnablesMatchingValues()
    {
        var rule = new FeatureRule { Default = false };
        rule.IncludeWhen.Add(new AttributeMatch("region", new HashSet<string> { "EU", "UK" }));

        var eu = new TargetingContext { Attributes = new Dictionary<string, string> { ["region"] = "EU" } };
        var us = new TargetingContext { Attributes = new Dictionary<string, string> { ["region"] = "US" } };

        Assert.Equal(FeatureDecision.Enabled, RuleEvaluator.Evaluate("F", rule, eu));
        Assert.Equal(FeatureDecision.Disabled, RuleEvaluator.Evaluate("F", rule, us));
    }

    [Fact]
    public void NoMatch_FallsBackToDefault()
    {
        var ctx = new TargetingContext { UserId = "nobody-special" };

        Assert.Equal(FeatureDecision.Enabled, RuleEvaluator.Evaluate("F", new FeatureRule { Default = true }, ctx));
        Assert.Equal(FeatureDecision.Disabled, RuleEvaluator.Evaluate("F", new FeatureRule { Default = false }, ctx));
        Assert.Equal(FeatureDecision.Unknown, RuleEvaluator.Evaluate("F", new FeatureRule { Default = null }, ctx));
    }

    [Fact]
    public void RolloutPercentage_Zero_NeverEnablesOnItsOwn()
    {
        var rule = new FeatureRule { Default = false, RolloutPercentage = 0 };

        for (var i = 0; i < 50; i++)
            Assert.Equal(FeatureDecision.Disabled,
                RuleEvaluator.Evaluate("F", rule, new TargetingContext { UserId = $"user-{i}" }));
    }

    [Fact]
    public void RolloutPercentage_Hundred_AlwaysEnablesAnyUser()
    {
        var rule = new FeatureRule { Default = false, RolloutPercentage = 100 };

        for (var i = 0; i < 50; i++)
            Assert.Equal(FeatureDecision.Enabled,
                RuleEvaluator.Evaluate("F", rule, new TargetingContext { UserId = $"user-{i}" }));
    }

    [Fact]
    public void RolloutPercentage_WithoutUserId_NeverAppliesOnItsOwn()
    {
        var rule = new FeatureRule { Default = false, RolloutPercentage = 100 };

        Assert.Equal(FeatureDecision.Disabled, RuleEvaluator.Evaluate("F", rule, new TargetingContext()));
    }

    [Fact]
    public void RolloutPercentage_IsDeterministic_ForTheSameFeatureAndUser()
    {
        var rule = new FeatureRule { Default = false, RolloutPercentage = 50 };
        var ctx = new TargetingContext { UserId = "stable-user" };

        var first = RuleEvaluator.Evaluate("F", rule, ctx);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, RuleEvaluator.Evaluate("F", rule, ctx));
    }

    [Fact]
    public void RolloutPercentage_CanDifferAcrossFeatures_ForTheSameUser()
    {
        // Not guaranteed for any single pair, but across 50 feature names the odds every
        // single one matches the baseline are astronomically small (0.5^50) if the
        // feature name is genuinely mixed into the hash rather than ignored.
        var rule = new FeatureRule { Default = false, RolloutPercentage = 50 };
        var ctx = new TargetingContext { UserId = "stable-user" };

        var baseline = RuleEvaluator.Evaluate("FeatureA", rule, ctx);
        var anyDifferent = Enumerable.Range(0, 50)
            .Select(i => RuleEvaluator.Evaluate($"Feature{i}", rule, ctx))
            .Any(d => d != baseline);

        Assert.True(anyDifferent);
    }

    [Fact]
    public void RolloutPercentage_ApproximatesTargetAcrossManyUsers()
    {
        var rule = new FeatureRule { Default = false, RolloutPercentage = 50 };
        const int sampleSize = 5000;

        var enabledCount = Enumerable.Range(0, sampleSize)
            .Count(i => RuleEvaluator.Evaluate("RolloutFeature", rule, new TargetingContext { UserId = $"user-{i}" })
                        == FeatureDecision.Enabled);

        var proportion = enabledCount / (double)sampleSize;

        // Generous band (~14 standard deviations at n=5000) to keep this non-flaky;
        // it's checking "roughly half", not precision.
        Assert.InRange(proportion, 0.40, 0.60);
    }
}
