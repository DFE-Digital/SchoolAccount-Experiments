using System.Collections.ObjectModel;
using SchoolAccount.QueryEngine.Core.Extension;

namespace SchoolAccount.QueryEngine.Core.Models;

public record Filterable(string Field, string DisplayName)
{
    public Collection<FilterableItem> Values { get; init; } = [];

    public bool AnySelectedChildren => Values.Any(HasSelection);

    private static bool HasSelection(FilterableItem item)
    {
        return item.IsSelected || (item.Children?.Any(HasSelection) ?? false);
    }

    public Collection<FilterableItem> GetValuesWithCountOnly(bool includeNullables = false)
    {
        return Values
            .Where(x => x.Count.HasValue && x.Count.Value > 0 || includeNullables && !x.Count.HasValue)
            .ToCollection();
    }

    public Collection<FilterableItem> GetSelectedValuesOnly()
    {
        return Values
            .Where(x => x.IsSelected)
            .ToCollection();
    }
}