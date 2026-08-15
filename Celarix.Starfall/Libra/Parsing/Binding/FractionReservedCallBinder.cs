using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class FractionReservedCallBinder : IReservedCallBinder
    {
        public LibraExpression Bind(ReservedCallSyntax syntax, LibraBinder binder)
        {
            if (syntax.Arguments.Count != 2)
            {
                throw binder.CreateValidationException(syntax,
                    ";frac requires exactly two arguments.");
            }

            var numerator = binder.BindExpression(syntax.Arguments[0]);
            var denominator = binder.BindExpression(syntax.Arguments[1]);
            var context = binder.Context;

            return new FractionExpression(numerator,
                denominator,
                context.ForegroundColor.WithOpacity(context.Opacity),
                context.BackgroundColor.WithOpacity(context.Opacity),
                binder.LibraId);
        }
    }
}
