using SchoolAccount.QueryEngine.Abstraction.Factories;

namespace SchoolAccount.QueryEngine.Abstraction.Pipelines;

public interface IFilterablePipeline
{
    IList<IFilterableFactory> Factories { get; }
}