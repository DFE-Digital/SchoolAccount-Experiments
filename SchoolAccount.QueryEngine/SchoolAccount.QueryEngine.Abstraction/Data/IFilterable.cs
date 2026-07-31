using System.Collections.ObjectModel;

namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IFilterable
{
    public Collection<IFilterableItem> Values { get; }
    public bool AnySelectedChildren { get; }
}