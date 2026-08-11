using Celarix.Starfall.Libra.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public sealed class LibraExpressionFactory<TInput>
    {
        public Func<TInput, LibraExpression> ExpressionFactory { get; }
        public Func<TInput, string> IdFactory { get; }

        public LibraExpressionFactory(Func<TInput, LibraExpression> expressionFactory, Func<TInput, string> idFactory)
        {
            ExpressionFactory = expressionFactory;
            IdFactory = idFactory;
        }
    }
}
