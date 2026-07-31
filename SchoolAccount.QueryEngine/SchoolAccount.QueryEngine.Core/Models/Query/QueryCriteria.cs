using SchoolAccount.QueryEngine.Abstraction;
using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Core.Models.Query;

public class QueryCriteria<TRow> : IQueryCriteria<TRow>
    where TRow : IQueryRow
{
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public DateOnlyRange Range { get; init; }
    public IList<IFilterRequest> Filter { get; init; } = [];
    public bool PopulateFilterOptions { get; init; } = true;
    public OrderFunction<TRow>? CustomOrderByFunction { get; init; }
}