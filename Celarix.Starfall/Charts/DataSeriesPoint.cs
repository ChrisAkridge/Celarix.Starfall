using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public readonly struct DataSeriesPoint : IEquatable<DataSeriesPoint>,
    IComparable<DataSeriesPoint>
{
    public BigInteger X { get; }
    public double? Y { get; }

    public DataSeriesPoint(BigInteger x, double? y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(DataSeriesPoint other)
    {
        // I'm going to take a wild and possibly bad guess and say equality only needs X.
        return other.X == X;
    }

    public int CompareTo(DataSeriesPoint other)
    {
        return X.CompareTo(other.X);
    }
}
