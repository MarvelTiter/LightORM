using System.Text;
using System.Collections.ObjectModel;
namespace LightORM;

public interface IExpressionResolver
{
    bool IsNot { get; }
    int NavigateDeep { get; set; }
    int Level { get; }
    Dictionary<string, Expression?>? ExpStores { get; set; }
    StringBuilder Sql { get; }
    bool UseNavigate { get; set; }
    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
    SqlResolveOptions Options { get; }
    Expression? NavigateWhereExpression { get; set; }
    Expression? Visit(Expression? expression);
    Expression? Body { get; }
    ReadOnlyCollection<ParameterExpression>? Parameters { get; }
}
