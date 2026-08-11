using AwesomeAssertions;
using SchoolAccount.FeatureManager.Core.Stores;

namespace SchoolAccount.FeatureManager.Core.Tests.InMemory;

public class InMemoryMutableFeatureStoreTests
{
    [Fact]
    public async Task If_feature_is_not_registered_return_empty()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();
        
        // Act
        var evaluation = await store.GetAsync("A");
        
        // Assert
        evaluation.Should().BeNull(because: "Feature A was not registered into the store.");
    }

    [Fact]
    public async Task If_feature_is_registered_it_returns()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();
        await store.SetAsync("A", true);

        // Act
        var evaluation = await store.GetAsync("A");

        // Assert
        evaluation.Should().NotBeNull(because: "Feature A was registered into the store.");
        evaluation.Enabled.Should().BeTrue(because: "Feature A was registered into the store in a enabled state.");
        evaluation.Rule.Should().BeNull(because: "Feature A was registered into the store without a rule.");
    }

    [Fact]
    public async Task If_feature_is_registered_and_updated_it_will_return_its_last_state()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();
        await store.SetAsync("A", true);
        await store.SetAsync("A", false);

        // Act
        var evaluation = await store.GetAsync("A");

        // Assert
        evaluation.Should().NotBeNull(because: "Feature A was registered into the store.");
        evaluation.Enabled.Should().BeFalse(because: "Feature A's last updated state was set up 'false'.");
        evaluation.Rule.Should().BeNull(because: "Feature A was registered into the store without a rule.");
    }

    [Fact]
    public async Task If_a_feature_is_removed_it_will_return_null_if_requested()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();
        await store.SetAsync("A", true);
        await store.RemoveAsync("A");

        // Act
        var evaluation = await store.GetAsync("A");

        // Assert
        evaluation.Should().BeNull(because: "Feature A was removed from the store.");
    }

    [Fact]
    public async Task If_a_feature_is_requested_to_be_removed_but_doesnt_exist_it_will_not_throw()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();

        // Act
        Action evaluation = async void () => await store.RemoveAsync("NeverSet");
        
        // Assert
        evaluation.Should().NotThrow(because: "No exceptions configured to throw.");
    }

    [Fact]
    public async Task Ensure_feature_lookup_is_case_insensitive()
    {
        // Assert
        var store = new InMemoryMutableFeatureStore();
        await store.SetAsync("A", true);

        // Act
        var evaluation = await store.GetAsync("a");

        // Assert
        evaluation.Should().NotBeNull(because: "We set up feature A as enabled into the provider on initialisation " +
                                               "even though we requested it as 'a'");
    }
}
