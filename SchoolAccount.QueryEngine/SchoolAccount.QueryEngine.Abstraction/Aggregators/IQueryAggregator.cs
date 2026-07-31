using SchoolAccount.QueryEngine.Abstraction.Data;
using SchoolAccount.QueryEngine.Abstraction.Pipelines;

namespace SchoolAccount.QueryEngine.Abstraction.Aggregators;

public interface IQueryAggregator
{
    Task<Result<IQueryPagedResult<TRow>>> Query<TRow>(
        IQueryFactoryPipeline<TRow> factoryPipeline,
        IFilterablePipeline filterPipeline,
        IQueryCriteria<TRow> criteria,
        CancellationToken cancellationToken = default
    )
        where TRow : IQueryRow;
}