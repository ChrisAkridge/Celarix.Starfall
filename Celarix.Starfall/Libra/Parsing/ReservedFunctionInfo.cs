using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed record ReservedFunctionInfo(string Name,
        int ArgumentCount,
        Func<LibraBuildContext, string?, LibraExpression[], LibraExpression> Resolver)
    {
        private static readonly IWhenNoneRule _whenNoneRule = new ReservedFunctionWhenNoneRule();

        public IWhenNoneRule WhenNoneRule => _whenNoneRule;
    }
}
