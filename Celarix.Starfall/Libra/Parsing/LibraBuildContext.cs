using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    public sealed record LibraBuildContext(
        string Source,
        SColor ForegroundColor,
        SColor BackgroundColor,
        double Opacity,
        FenceType? FenceType,
        IReadOnlyDictionary<string, Func<LibraExpression>> SubstitutionResolvers
    )
    {
        public LibraBuildContext Substitute(string substitutionKey, Func<LibraExpression> resolver)
        {
            var newResolvers = new Dictionary<string, Func<LibraExpression>>(SubstitutionResolvers)
            {
                [substitutionKey] = resolver
            };
            return this with { SubstitutionResolvers = newResolvers };
        }

        public LibraBuildContext Colors(SColor foregroundColor, SColor backgroundColor) => this with { ForegroundColor = foregroundColor, BackgroundColor = backgroundColor };
        public LibraBuildContext Foreground(SColor foreground) => this with { ForegroundColor = foreground };
        public LibraBuildContext Background(SColor background) => this with { BackgroundColor = background };

        public LibraExpression Build()
        {
            var lexer = new Lexer(Source);
            var parser = new LibraParser(lexer.Parse());
            var syntax = parser.Parse();
            LibraSyntaxValidator.Validate(syntax);
            var builder = new LibraExpressionBuilder();
            return builder.Build(syntax, this);
        }
    }
}
