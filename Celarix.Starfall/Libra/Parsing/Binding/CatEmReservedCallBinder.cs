using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class CatEmReservedCallBinder : IReservedCallBinder
    {
        public LibraExpression Bind(ReservedCallSyntax syntax, LibraBinder binder)
        {
            if (syntax.Arguments.Count < 2)
            {
                throw binder.CreateValidationException(syntax,
                    ";catEm requires a gap followed by at least one expression.");
            }

            var gapEm = binder.BindNumber(syntax.Arguments[0]);
            var children = syntax.Arguments
                .Skip(1)
                .Select(binder.BindExpression)
                .ToArray();

            return new RowExpression(children, gapEm, binder.LibraId);
        }
    }
}
