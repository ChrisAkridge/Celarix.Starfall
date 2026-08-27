using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed class ChartText
{
    public string SourceString { get; }
    public bool UseLibra { get; }

    public ChartText(string sourceString, bool useLibra = false)
    {
        SourceString = sourceString;
        UseLibra = useLibra;
    }

    public static ChartText String(string sourceString) => new(sourceString, useLibra: false);
    public static ChartText Libra(string sourceString) => new(sourceString, useLibra: true);

    public LibraLayoutResult Layout(LibraRenderingContext context, SColor textColor)
    {
        // "Wait, it's all Libra?"
        // "Always has been."

        if (!UseLibra)
        {
            var textExpression = LibraExpressions.Text(SourceString, textColor, SColor.Transparent);
            return textExpression.Layout(context);
        }
        else
        {
            var parsedExpression = LibraExpression.Parse(SourceString)
                .Foreground(textColor)
                .Build();
            return parsedExpression.Layout(context);
        }
    }
}
