using Celarix.Starfall.Charts;
using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Celarix.Starfall.Tests.Charts;

public sealed class ChartCoreTests
{
    [Fact]
    public void AtomicUpdate_CommitsOnceAndRollsBackOnFailure()
    {
        var properties = new TestProperties();
        var events = 0;
        properties.PropertiesChanged += (_, _) => events++;

        properties.UpdatePropertiesAtomic(() => { properties.Minimum = 5; properties.Maximum = 8; });
        Assert.Equal(1, events);
        Assert.Equal((5, 8), (properties.Minimum, properties.Maximum));

        Assert.Throws<ArgumentException>(() => properties.UpdatePropertiesAtomic(() =>
        {
            properties.Minimum = 20;
            properties.Maximum = 10;
        }));
        Assert.Equal((5, 8), (properties.Minimum, properties.Maximum));
        Assert.Equal(1, events);
    }

    [Fact]
    public void AtomicUpdate_RejectsNestingAndRestoresOriginalValues()
    {
        var properties = new TestProperties();
        Assert.Throws<InvalidOperationException>(() => properties.UpdatePropertiesAtomic(() =>
        {
            properties.Minimum = 2;
            properties.UpdatePropertiesAtomic(() => properties.Maximum = 3);
        }));
        Assert.Equal((0, 1), (properties.Minimum, properties.Maximum));
    }

    [Fact]
    public void DataSeriesPoint_DefaultAndExactEqualityHaveDocumentedSemantics()
    {
        var first = new DataSeriesPoint(4, 10);
        var second = new DataSeriesPoint(4, 20);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.False(DataSeriesPoint.ExactComparer.Equals(first, second));
        Assert.Single(new HashSet<DataSeriesPoint> { first, second });
    }

    [Fact]
    public void DataSeries_UsesStableStatisticsAndNormalizesNonFiniteValues()
    {
        var series = new DataSeries([
            new(0, 1_000_000_000_000d + 1),
            new(1, 1_000_000_000_000d + 2),
            new(2, 1_000_000_000_000d + 3),
            new(3, double.NaN)]);

        Assert.Equal(0d, series[3].Y);
        Assert.True(series.PopulationVariance > 0d);
        var variance = series.PopulationVariance;
        series.RecalculateStatistics();
        Assert.Equal(variance, series.PopulationVariance, 6);
    }

    [Fact]
    public void DataSeries_RangeLookupHandlesEnormousSparseRangesInOrder()
    {
        var far = BigInteger.Pow(10, 1000);
        var series = new DataSeries([new(far, 2), new(-far, 1), new(0, null)]);
        var points = series.GetPointsInRange(new XRange(-far, far));
        Assert.Equal(2, points.Count);
        Assert.Equal(-far, points[0].X);
        Assert.Equal(far, points[1].X);
    }

    [Fact]
    public void Resolution_ProducesContiguousBucketsAndStableAverage()
    {
        var series = new DataSeries([new(0, 1e12 + 1), new(1, 1e12 + 2), new(2, 1e12 + 3)]);
        var source = new DataSeriesDataSource(series, new StandardResolutionStrategy());
        var aggregate = Assert.IsType<AggregatedDataPoint>(DataResolver.Resolve(source, new XRange(0, 2), 1).Single());
        Assert.Equal(1e12 + 2, aggregate.AverageY, 10);
        Assert.Equal(3e12 + 6, aggregate.SumY);
    }

    [Fact]
    public void LabelFitExtentMultiplier_ReservesProportionalMajorAxisSpace()
    {
        LibraLayoutResult LabelFactory(BigInteger _) => new(
            Array.Empty<LibraRenderable>(), new SRectF(0, 0, 10, 4), 0, 0);
        SRectF SlotFactory(BigInteger x) => new((double)x * 12d, 0, 0, 0);

        var naturalFit = ChartHelpers.FitLabelsForAxis(
            new XRange(0, 2), LabelFactory, SlotFactory, Side.Bottom, 0, 1d);
        var expandedFit = ChartHelpers.FitLabelsForAxis(
            new XRange(0, 2), LabelFactory, SlotFactory, Side.Bottom, 0, 1.4d);

        Assert.Equal(3, naturalFit.Count);
        Assert.Equal(2, expandedFit.Count);
        Assert.All(expandedFit, label => Assert.Equal(10d, label.LibraLayoutResult.Bounds.Width));
    }

    [Fact]
    public void ChartInsets_ApplyNormallyWhenTheyFit()
    {
        var result = new ChartInsets(10, 20, 30, 40).ApplyTo(new SRectF(100, 200, 200, 150));

        Assert.Equal(new SRectF(110, 220, 160, 90), result);
    }

    [Fact]
    public void ChartInsets_CollapseProportionallyWithoutNegativeDimensions()
    {
        var result = new ChartInsets(30, 80, 70, 20).ApplyTo(new SRectF(10, 20, 50, 40));

        Assert.Equal(new SRectF(25, 52, 0, 0), result);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ChartInsets_RejectInvalidValues(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChartInsets(value, 0, 0, 0));
    }

    private sealed class TestProperties : ChartPropertyBase
    {
        private int _minimum;
        private int _maximum = 1;
        public int Minimum { get => _minimum; set => SetProperty(value, _minimum, v => _minimum = v); }
        public int Maximum { get => _maximum; set => SetProperty(value, _maximum, v => _maximum = v); }

        protected override bool Valid([NotNullWhen(false)] out Exception? ex)
        {
            ex = _minimum <= _maximum ? null : new ArgumentException("Minimum must not exceed maximum.");
            return ex is null;
        }
    }
}
