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

    public override bool Equals(object? obj) => obj is DataSeriesPoint other && Equals(other);

    public override int GetHashCode() => X.GetHashCode();

    public static bool operator ==(DataSeriesPoint left, DataSeriesPoint right) => left.Equals(right);
    public static bool operator !=(DataSeriesPoint left, DataSeriesPoint right) => !left.Equals(right);

    public static IEqualityComparer<DataSeriesPoint> ExactComparer { get; } = new ExactPointComparer();

    private sealed class ExactPointComparer : IEqualityComparer<DataSeriesPoint>
    {
        public bool Equals(DataSeriesPoint x, DataSeriesPoint y) => x.X == y.X && x.Y == y.Y;
        public int GetHashCode(DataSeriesPoint obj) => HashCode.Combine(obj.X, obj.Y);
    }

    public int CompareTo(DataSeriesPoint other)
    {
        return X.CompareTo(other.X);
    }
}
