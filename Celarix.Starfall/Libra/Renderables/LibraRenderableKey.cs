using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables
{
    public readonly record struct LibraRenderableKey(
        Guid ExpressionId,
        string Role)
    {
        public override string ToString() => $"{ExpressionId:N}-{Role}";
    }
}
