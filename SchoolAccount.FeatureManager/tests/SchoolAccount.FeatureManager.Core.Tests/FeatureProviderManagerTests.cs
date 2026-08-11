using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Builders;
using SchoolAccount.FeatureManager.Core.Context;
using SchoolAccount.FeatureManager.Core.Extensions;

namespace SchoolAccount.FeatureManager.Core.Tests;

public class FeatureProviderProviderManagerTests : DiTestBase
{
    [Fact]
    public async Task ReturnsDefault_WhenNoProviderHasAnOpinion()
    {
        var manager = Build(b => b.AddProvider(new FakeProvider(FeatureDecision.Unknown)), defaultEnabled: true);
        Assert.True(await manager.IsEnabledAsync("Anything"));
    }

    [Fact]
    public async Task FirstDefiniteProvider_ShortCircuitsThePipeline()
    {
        var first = new FakeProvider(FeatureDecision.Unknown);
        var second = new FakeProvider(FeatureDecision.Enabled);
        var third = new FakeProvider(FeatureDecision.Disabled);

        var manager = Build(b => b.AddProvider(first).AddProvider(second).AddProvider(third));

        Assert.True(await manager.IsEnabledAsync("X"));
        Assert.Equal(1, first.CallCount);
        Assert.Equal(1, second.CallCount);
        Assert.Equal(0, third.CallCount); // never reached — the pipeline stopped at `second`
    }

    [Fact]
    public async Task RegistrationOrder_IsPriority()
    {
        var first = new FakeProvider(FeatureDecision.Disabled);
        var second = new FakeProvider(FeatureDecision.Enabled);

        var manager = Build(b => b.AddProvider(first).AddProvider(second));

        Assert.False(await manager.IsEnabledAsync("X")); // first wins even though second disagrees
    }

    [Fact]
    public async Task ExplicitTargetingOverload_OverridesTheAmbientAccessor()
    {
        var provider = new DelegateProvider(ctx =>
            ctx.Targeting.UserId == "alice" ? FeatureDecision.Enabled : FeatureDecision.Disabled);

        var manager = Build(
            b => b.AddProvider(provider),
            targetingAccessor: new FixedTargetingContextAccessor(new TargetingContext { UserId = "bob" }));

        Assert.False(await manager.IsEnabledAsync("X"));                                       // ambient: bob
        Assert.True(await manager.IsEnabledAsync("X", new TargetingContext { UserId = "alice" })); // explicit override
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowsOnInvalidFeatureName(string featureName)
    {
        var manager = Build(_ => { });
        await Assert.ThrowsAsync<ArgumentException>(() => manager.IsEnabledAsync(featureName));
    }

    [Fact]
    public async Task ExplicitOverload_ThrowsOnNullTargetingContext()
    {
        var manager = Build(_ => { });
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.IsEnabledAsync("X", null!));
    }

    private IFeatureProviderManager Build(
        Action<FeatureFlagsBuilder> configure,
        bool defaultEnabled = false,
        ITargetingContextAccessor? targetingAccessor = null)
    {
        var sp = BuildScope(services =>
        {
            var builder = services.AddFeatureFlags(o => o.DefaultResponse = defaultEnabled);
            configure(builder);

            if (targetingAccessor is not null)
                services.AddScoped(_ => targetingAccessor); // registered after AddFeatureFlags's TryAdd, so it wins
        });

        return sp.GetRequiredService<IFeatureProviderManager>();
    }
}
