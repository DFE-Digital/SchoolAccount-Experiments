using System.Linq.Expressions;

namespace SchoolAccount.QueryEngine.Abstraction;

public class FieldSelector : Dictionary<string, LambdaExpression>;
