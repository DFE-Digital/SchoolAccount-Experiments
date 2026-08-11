using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Abstraction;

/// <summary>
/// Supplies the ambient <see cref="TargetingContext"/> for the current evaluation.
/// Same pattern as <c>IHttpContextAccessor</c>: an ASP.NET Core request populates it
/// from the user, a background job supplies its own.
/// </summary>
public interface ITargetingContextAccessor
{
    ValueTask<TargetingContext> GetContextAsync(CancellationToken cancellationToken = default);
}