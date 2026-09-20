using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal interface IReservedCallBinder
    {
        LibraExpression Bind(ReservedCallSyntax syntax, LibraBinder binder);
    }
}
