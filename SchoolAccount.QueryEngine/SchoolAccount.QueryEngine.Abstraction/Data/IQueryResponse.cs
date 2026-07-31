namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IQueryResponse<out TRow>
    where TRow : IQueryRow
{
    public int Count { get; }
    public IEnumerable<TRow> Payload { get; }
}