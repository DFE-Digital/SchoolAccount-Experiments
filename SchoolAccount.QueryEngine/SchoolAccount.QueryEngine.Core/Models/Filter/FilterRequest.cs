using System.Collections.ObjectModel;
using SchoolAccount.QueryEngine.Abstraction.Data;
using SchoolAccount.QueryEngine.Abstraction.Enums;

namespace SchoolAccount.QueryEngine.Core.Models.Filter;

public class FilterRequest : IFilterRequest
{
    public JoinType Join { get; init; } = JoinType.And;
    public Collection<IFilterRequest> Children { get; init; } = [];

    public string Field { get; init; } = null!;
    public ComparisonType Operator { get; init; } = ComparisonType.Equals;
    public object? Value { get; init; }
}