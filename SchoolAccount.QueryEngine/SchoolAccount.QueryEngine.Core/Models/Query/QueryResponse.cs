using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Core.Models.Query;

public record QueryResponse<TRow>(int Count, IEnumerable<TRow> Payload)
    where TRow : IQueryRow
{
    public static implicit operator QueryResponse<TRow>(Tuple<int, IEnumerable<TRow>> tuple) =>
        new(tuple.Item1, tuple.Item2);
    public static implicit operator QueryResponse<TRow>((int, IEnumerable<TRow>) tuple) =>
        new(tuple.Item1, tuple.Item2);
}