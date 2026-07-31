namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IQueryCriteria<TRow>
    where TRow : IQueryRow
{
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public DateOnlyRange Range { get; init; }
    public IList<IFilterRequest> Filter { get; init; }
    public bool PopulateFilterOptions { get; init; }
    public OrderFunction<TRow>? CustomOrderByFunction { get; init; }
}