using Microsoft.Extensions.DependencyInjection;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>A no-op <see cref="IServiceProvider"/> for tests that need to hand one to a <see cref="FeatureContext"/>.</summary>
internal static class TestServices
{
    public static readonly IServiceProvider Empty = new ServiceCollection().BuildServiceProvider();
}