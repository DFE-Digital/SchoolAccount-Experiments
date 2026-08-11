using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Builders;
using SchoolAccount.FeatureManager.Core.Extensions;

namespace SchoolAccount.FeatureManager.Core.Tests;

public class FeatureAdminTests : DiTestBase
{
    [Fact]
    public async Task NoSourceName_Throws_WhenNoSourcesRegistered()
    {
        var admin = BuildAdmin(_ => { });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => admin.SetAsync("X", true));
        Assert.Contains("No writable feature sources", ex.Message);
    }

    [Fact]
    public async Task NoSourceName_Works_WithExactlyOneSource()
    {
        var admin = BuildAdmin(b => b.AddMutableInMemory("Only"));

        await admin.SetAsync("X", true); // should not throw
    }

    [Fact]
    public async Task NoSourceName_Throws_WithMultipleSources_AndListsThem()
    {
        var admin = BuildAdmin(b => b.AddMutableInMemory("A").AddMutableInMemory("B"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => admin.SetAsync("X", true));
        Assert.Contains("A", ex.Message);
        Assert.Contains("B", ex.Message);
    }

    [Fact]
    public async Task BySourceName_Throws_ForAnUnregisteredSource()
    {
        var admin = BuildAdmin(b => b.AddMutableInMemory("A"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => admin.SetAsync("DoesNotExist", "X", true));

        Assert.Contains("DoesNotExist", ex.Message);
        Assert.Contains("A", ex.Message); // tells you what IS registered
    }

    [Fact]
    public async Task SetRuleAsync_Throws_WhenSourceDoesNotSupportRules()
    {
        // The mutable in-memory source has SupportsRules == false.
        var admin = BuildAdmin(b => b.AddMutableInMemory("Only"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => admin.SetRuleAsync("Only", "X", new FeatureRule { Default = true }));

        Assert.Contains("does not support rule-based targeting", ex.Message);
    }

    [Fact]
    public void Sources_ReflectsRegisteredSourcesAndTheirCapabilities()
    {
        var admin = BuildAdmin(b => b.AddMutableInMemory("Only"));

        var source = Assert.Single(admin.Sources);
        Assert.Equal("Only", source.Name);
        Assert.False(source.SupportsRules);
    }

    private IFeatureManager BuildAdmin(Action<FeatureFlagsBuilder> configure)
    {
        var sp = BuildScope(services =>
        {
            var builder = services.AddFeatureFlags();
            configure(builder);
        });

        return sp.GetRequiredService<IFeatureManager>();
    }
}
