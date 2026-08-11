using AwesomeAssertions;
using SchoolAccount.FeatureManager.Core.Context;
using SchoolAccount.FeatureManager.Core.Providers;

namespace SchoolAccount.FeatureManager.Core.Tests.InMemory;

public class InMemoryFeatureProviderTests
{
    [Fact]
    public async Task Ensure_if_the_feature_is_true_it_returns_a_enabled_decision()
    {
        // Arrange
        var provider = new InMemoryFeatureProvider(new Dictionary<string, bool> { ["A"] = true });
        var context = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);

        // Act
        var evaluation = await provider.EvaluateAsync(context);
        
        // Assert
        evaluation.Should().Be(FeatureDecision.Enabled,
            because: "We set up feature A as enabled into the provider on initialisation");
    }

    [Fact]
    public async Task Ensure_if_the_feature_is_false_it_returns_a_disabled_decision()
    {
        // Arrange
        var provider = new InMemoryFeatureProvider(new Dictionary<string, bool> { ["A"] = false });
        var context = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);
        
        // Act
        var evaluation = await provider.EvaluateAsync(context);
        
        // Assert
        evaluation.Should().Be(FeatureDecision.Disabled,
            because: "We set up feature A as disabled into the provider on initialisation");
    }

    [Fact]
    public async Task When_a_feature_request_is_not_registered_its_deferred_as_unknown()
    {
        // Arrange
        var provider = new InMemoryFeatureProvider(new Dictionary<string, bool>());
        var context = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);

        // Act
        var evaluation = await provider.EvaluateAsync(context);
        
        // Assert
        evaluation.Should().Be(FeatureDecision.Unknown,
            because: "We never initiated A into the provider on initialisation");
    }

    [Fact]
    public async Task Ensure_that_feature_checks_are_case_insensitive()
    {
        // Arrange
        var provider = new InMemoryFeatureProvider(new Dictionary<string, bool> { ["A"] = true });
        var context = new FeatureContext("a", TestServices.Empty, TargetingContext.Empty);

        // Act
        var evaluation = await provider.EvaluateAsync(context);
        
        // Assert
        evaluation.Should().Be(FeatureDecision.Enabled,
            because: "We set up feature A as enabled into the provider on initialisation even though we requested it " +
                     "as 'a'");
    }
}
