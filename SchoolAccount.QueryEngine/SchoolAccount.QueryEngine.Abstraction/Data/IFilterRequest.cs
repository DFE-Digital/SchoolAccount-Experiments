using System.Collections.ObjectModel;
using SchoolAccount.QueryEngine.Abstraction.Enums;

namespace SchoolAccount.QueryEngine.Abstraction.Data;

public interface IFilterRequest
{
    public JoinType Join { get; }
    public Collection<IFilterRequest> Children { get; }

    public string Field { get; }
    public ComparisonType Operator { get; }
    public object? Value { get; }
}