using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Abstraction.Factories;

public interface IFilterableFactory
{
    Task<List<IFilterable>> GetAvailableFiltersAsync<TRow>(IQueryable<TRow>? baseQuery = null)
        where TRow : IQueryRow;
}