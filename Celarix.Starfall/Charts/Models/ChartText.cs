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
    public static readonly ChartText Empty = new(string.Empty, useLibra: false);

    public string SourceString { get; }
    public bool UseLibra { get; }

    public ChartText(string sourceString, bool useLibra = false)
    {
        SourceString = sourceString;
        UseLibra = useLibra;
    }

    public static ChartText String(string sourceString) => new(sourceString, useLibra: false);
    public static ChartText Libra(string sourceString) => new(sourceString, useLibra: true);

    public static ChartText Concat(ChartText a, ChartText b)
    {
        if (a.UseLibra || b.UseLibra)
        {
            var aString = a.UseLibra ? a.SourceString : $"\"{a.SourceString}\"";
            var bString = b.UseLibra ? b.SourceString : $"\"{b.SourceString}\"";
            return new ChartText($";catEm(1, {aString}, {bString})", useLibra: true);
        }
        else
        {
            return new ChartText($"{a.SourceString}{b.SourceString}", useLibra: false);
        }
    }

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
