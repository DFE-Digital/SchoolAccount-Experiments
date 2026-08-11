using Microsoft.Extensions.DependencyInjection;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>
/// Shared helper for DI-based tests: builds a container, tracks the root provider and
/// the scope for disposal (LIFO), and hands back the scoped <see cref="IServiceProvider"/>
/// to resolve from — so tests exercise the real registration extensions rather than
/// reaching into internal types.
/// </summary>
public abstract class DiTestBase : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    protected IServiceProvider BuildScope(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        _disposables.Add(scope);
        _disposables.Add(provider);
        return scope.ServiceProvider;
    }

    public void Dispose()
    {
        for (var i = _disposables.Count - 1; i >= 0; i--)
            _disposables[i].Dispose();
    }
}
