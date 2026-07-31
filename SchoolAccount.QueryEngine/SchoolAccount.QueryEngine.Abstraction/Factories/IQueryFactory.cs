using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Abstraction.Factories;

public interface IQueryFactory<TRow> 
    where TRow : IQueryRow
{
    Type? TypeBeingRegistered { get; }

    Task<IQueryResponse<TRow>> Query(IQueryCriteria<TRow> criteria, FieldSelectorMapping mappings,
        CancellationToken cancellationToken);
}