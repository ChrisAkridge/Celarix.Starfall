using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Binding;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class LibraExpressionBuilder
    {
        public LibraExpression Build(ExpressionSyntax syntax,
            LibraBuildContext context,
            string? libraId = null)
        {
            return new LibraBinder(context, libraId).BindExpression(syntax);
        }
    }
}
