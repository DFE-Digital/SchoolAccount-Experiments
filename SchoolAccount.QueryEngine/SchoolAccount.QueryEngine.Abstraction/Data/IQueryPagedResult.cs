using System.Collections.ObjectModel;

namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IQueryPagedResult<out TRow>
{
    public DateTime GeneratedDate { get; }

    public DateOnlyRange Range { get; }
    public Collection<IFilterable> Filter { get; }
    public IReadOnlyCollection<TRow> Payload { get; }

    public int PageCount { get; }
    public int TotalItemCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
    public bool IsFirstPage { get; }
    public bool IsLastPage { get; }
    public int FirstItemOnPage { get; }
    public int LastItemOnPage { get; }
}