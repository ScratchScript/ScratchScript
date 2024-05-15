using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IUnaryHandler
{
    public ExpressionValue GetUnaryExpression(AddOperator op, ExpressionValue expression);
}