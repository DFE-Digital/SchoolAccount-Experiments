using SchoolAccount.QueryEngine.Abstraction.Data;

namespace SchoolAccount.QueryEngine.Abstraction;

public delegate IOrderedEnumerable<T> OrderFunction<T>(IList<T> query)
    where T : IQueryRow;