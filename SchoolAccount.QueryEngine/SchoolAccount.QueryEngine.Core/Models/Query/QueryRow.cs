using SchoolAccount.QueryEngine.Abstraction;
using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Core.Models.Query;

public class QueryRow : Node<string>, IQueryRow
{
    public string? Description { get; init; }
    public DateOnly? SortDate { get; init; }
    public Node<int> Type { get; init; } = 0;
    public DateTime? LastUpdated { get; init; }
    public ExtensionNode<int>? Status { get; init; }
    public IEnumerable<ExtensionNode<long>> Types { get; init; } = [];
    public IEnumerable<ExtensionNode<long>> Tags { get; init; } = [];
}