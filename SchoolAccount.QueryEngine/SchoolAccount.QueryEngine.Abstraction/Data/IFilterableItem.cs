using System.Collections.ObjectModel;

namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IFilterableItem
{
    public string DisplayName { get; }
    public string Value { get; }
    public bool IsSelected { get; }
    public Collection<IFilterableItem>? Children { get; }
    public int? Count { get; }
}