using SchoolAccount.FeatureManager.Core.Context;
using SchoolAccount.FeatureManager.Core.Providers;

namespace SchoolAccount.FeatureManager.Core.Tests;

public class StoredFeatureProviderTests
{
    [Fact]
    public async Task ReturnsUnknown_WhenSourceHasNoData()
    {
        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?>());
        var provider = new StoredFeatureProvider(source);

        var context = new FeatureContext("Missing", TestServices.Empty, TargetingContext.Empty);
        Assert.Equal(FeatureDecision.Unknown, await provider.EvaluateAsync(context));
    }

    [Theory]
    [InlineData(true, FeatureDecision.Enabled)]
    [InlineData(false, FeatureDecision.Disabled)]
    public async Task ReturnsThePlainBool_WhenNoRuleIsAttached(bool enabled, FeatureDecision expected)
    {
        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?> { ["A"] = new StoredFeature(enabled, null) });
        var provider = new StoredFeatureProvider(source);

        var context = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);
        Assert.Equal(expected, await provider.EvaluateAsync(context));
    }

    [Fact]
    public async Task EvaluatesTheRule_AgainstTheContextsTargeting_WhenARuleIsAttached()
    {
        var rule = new FeatureRule { Default = false };
        rule.IncludeUsers.Add("alice");

        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?> { ["A"] = new StoredFeature(null, rule) });
        var provider = new StoredFeatureProvider(source);

        var forAlice = new FeatureContext("A", TestServices.Empty, new TargetingContext { UserId = "alice" });
        var forBob = new FeatureContext("A", TestServices.Empty, new TargetingContext { UserId = "bob" });

        Assert.Equal(FeatureDecision.Enabled, await provider.EvaluateAsync(forAlice));
        Assert.Equal(FeatureDecision.Disabled, await provider.EvaluateAsync(forBob));
    }
}
