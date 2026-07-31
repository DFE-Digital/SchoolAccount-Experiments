namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IQueryRow
{
    public string? Id { get; }
    public string? Name { get; }
    public string? Description { get; }
    public DateOnly? SortDate { get; }
    public Node<int> Type { get; }
    public DateTime? LastUpdated { get; }
    public ExtensionNode<int>? Status { get; }
    public IEnumerable<ExtensionNode<long>> Types { get; }
    public IEnumerable<ExtensionNode<long>> Tags { get; }
}
