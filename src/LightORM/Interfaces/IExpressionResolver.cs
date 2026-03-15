using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;
namespace LightORM;

public interface IExpressionResolver
{
    SqlResolveOptions Options { get; }
    ResolveContext Context { get; }
    bool IsNot { get; }
    int NavigateDeep { get; set; }
    int Level { get; }
    Dictionary<string, Expression?>? ExpStores { get; set; }
    StringBuilder Sql { get; }
    bool UseNavigate { get; set; }
    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
    Expression[]? NavigateWhereExpression { get; set; }
    Expression? Visit(Expression? expression);
    Expression? Body { get; }
    BinaryExpression? CurrentBinary { get; }
    ReadOnlyCollection<ParameterExpression>? Parameters { get; }
}
