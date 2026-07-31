namespace SchoolAccount.QueryEngine.Core.Models;

public record Error(string Code, string Description, ErrorType Type, string? Property = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("General.Null", "Null value was provided", ErrorType.Failure);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) => new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    public static Error Validation(string code, string description, string? property = null) =>
        new(code, description, ErrorType.Validation, property);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
}