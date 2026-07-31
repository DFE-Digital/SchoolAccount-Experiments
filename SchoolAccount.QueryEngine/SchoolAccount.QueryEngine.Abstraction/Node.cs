namespace SchoolAccount.QueryEngine.Abstraction;

public class Node<TId>
{
    public TId? Id { get; init; }
    public string? Name { get; init; }

    protected Node() { }

    public Node(TId id)
    {
        Id = id;
    }

    public Node(string name)
    {
        Id = default;
        Name = name;
    }

    public Node(Enum enumValue)
    {
        Id = (TId)(object)enumValue;
        Name = Enum.GetName(enumValue.GetType(), enumValue) ?? throw new InvalidCastException();
    }

    public static implicit operator Node<TId>(TId value)
    {
        return new Node<TId>(value);
    }

    public static implicit operator Node<TId>(Enum enumValue)
    {
        return new Node<TId>(enumValue);
    }

    public static implicit operator TId?(Node<TId> node)
    {
        return node.Id;
    }

    public static implicit operator Node<TId>(string name)
    {
        return new Node<TId>(name);
    }
}