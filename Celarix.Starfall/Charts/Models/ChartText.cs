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
}
