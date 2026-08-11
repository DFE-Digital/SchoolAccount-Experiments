using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Context;
using SchoolAccount.FeatureManager.Core.Extensions;
using SchoolAccount.FeatureManager.Core.Stores;

namespace SchoolAccount.FeatureManager.Core.Tests;

public class DependencyInjectionRegistrationTests : DiTestBase
{
    [Fact]
    public void AddFeatureFlags_RegistersManagerAdminAndDefaultTargetingAccessor()
    {
        var sp = BuildScope(services => services.AddFeatureFlags());

        Assert.NotNull(sp.GetService<IFeatureProviderManager>());
        Assert.NotNull(sp.GetService<IFeatureManager>());
        Assert.IsType<EmptyTargetingContextAccessor>(sp.GetService<ITargetingContextAccessor>());
    }

    [Fact]
    public void RegisterSource_ThrowsOnDuplicateName()
    {
        var services = new ServiceCollection();
        var builder = services.AddFeatureFlags();
        builder.RegisterSource("Dup", supportsRules: false);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.RegisterSource("Dup", supportsRules: true));
        Assert.Contains("Dup", ex.Message);
    }

    [Fact]
    public void AddMutableInMemory_CalledTwiceWithSameName_Throws()
    {
        var services = new ServiceCollection();
        var builder = services.AddFeatureFlags();
        builder.AddMutableInMemory("Same");

        Assert.Throws<InvalidOperationException>(() => builder.AddMutableInMemory("Same"));
    }

    /// <summary>
    /// Regression test for a bug caught while writing these tests: <c>AddMutableInMemory</c>
    /// used to register its backing store as an <em>unkeyed</em> singleton, so calling it
    /// twice with different names would make both keyed stores silently resolve to the
    /// same (last-registered) instance. Confirms the fix: two named sources are genuinely
    /// independent, even for the same feature name.
    /// </summary>
    [Fact]
    public async Task TwoNamedMutableInMemorySources_StayIndependent_ForTheSameFeatureName()
    {
        // Arrange
        var sp = BuildScope(services => services
            .AddFeatureFlags()
            .AddMutableInMemory("A")
            .AddMutableInMemory("B"));

        var admin = sp.GetRequiredService<IFeatureManager>();
        await admin.SetAsync("A", "SharedName", true);
        await admin.SetAsync("B", "SharedName", false);

        var storeA = sp.GetRequiredKeyedService<InMemoryMutableFeatureStore>("A");
        var storeB = sp.GetRequiredKeyedService<InMemoryMutableFeatureStore>("B");

        // Act
        var storeAEvaluation = await storeA.GetAsync("SharedName");
        var storeBEvaluation = await storeB.GetAsync("SharedName");

        // Assert
        storeA.Should().NotBeSameAs(storeB);
        
        storeAEvaluation.Should().NotBeNull();
        storeAEvaluation.Enabled.Should().BeTrue();
        storeBEvaluation.Should().NotBeNull();
        storeBEvaluation.Enabled.Should().BeFalse();
    }
}
