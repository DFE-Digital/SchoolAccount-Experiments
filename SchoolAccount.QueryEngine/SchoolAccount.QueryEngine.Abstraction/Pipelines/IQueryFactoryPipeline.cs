using SchoolAccount.QueryEngine.Abstraction.Data;
using SchoolAccount.QueryEngine.Abstraction.Factories;

namespace SchoolAccount.QueryEngine.Abstraction.Pipelines;

public interface IQueryFactoryPipeline<TRow> 
    where TRow : IQueryRow
{
    IList<IQueryFactory<TRow>> Factories { get; }
}