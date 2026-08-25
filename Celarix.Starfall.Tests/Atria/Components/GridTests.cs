using Celarix.Starfall.Layout.Atria.Components;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Tests.Atria.Components;

public sealed class GridTests
{
    [Fact]
    public void GetOuterCellBounds_ReturnsFullCellSlot()
    {
        var grid = new Grid(new SSize(4, 3), new SSizeF(20d, 10d), 2d, SColor.White);

        var bounds = grid.GetOuterCellBounds(new SPointF(100d, 200d), new SPoint(2, 1));

        AssertRect(bounds, 140d, 210d, 20d, 10d);
    }

    [Fact]
    public void GetInnerCellBounds_ReturnsDrawableAreaInsideGridLines()
    {
        var grid = new Grid(new SSize(4, 3), new SSizeF(20d, 10d), 2d, SColor.White);

        var bounds = grid.GetInnerCellBounds(new SPointF(100d, 200d), new SPoint(2, 1));

        AssertRect(bounds, 142d, 212d, 17.5d, 7.333333333333333d);
    }

    [Fact]
    public void GetContentCellBounds_AppliesContentInsetToInnerBounds()
    {
        var grid = new Grid(new SSize(4, 3), new SSizeF(20d, 10d), 2d, SColor.White);

        var bounds = grid.GetContentCellBounds(new SPointF(100d, 200d), new SPoint(2, 1), 0.2d);

        AssertRect(bounds, 143.75d, 212.73333333333332d, 14d, 5.866666666666666d);
    }

    [Theory]
    [InlineData(100d, 200d, 0, 0)]
    [InlineData(119.999d, 209.999d, 0, 0)]
    [InlineData(120d, 210d, 1, 1)]
    [InlineData(179.999d, 229.999d, 3, 2)]
    public void TryGetCellAt_PointInsideGrid_ReturnsCell(double x,
        double y,
        int expectedX,
        int expectedY)
    {
        var grid = new Grid(new SSize(4, 3), new SSizeF(20d, 10d), 2d, SColor.White);

        var result = grid.TryGetCellAt(new SPointF(100d, 200d), new SPointF(x, y), out var cell);

        Assert.True(result);
        Assert.Equal(expectedX, cell.X);
        Assert.Equal(expectedY, cell.Y);
    }

    [Theory]
    [InlineData(99.999d, 200d)]
    [InlineData(100d, 199.999d)]
    [InlineData(180d, 200d)]
    [InlineData(100d, 230d)]
    public void TryGetCellAt_PointOutsideGrid_ReturnsFalse(double x,
        double y)
    {
        var grid = new Grid(new SSize(4, 3), new SSizeF(20d, 10d), 2d, SColor.White);

        var result = grid.TryGetCellAt(new SPointF(100d, 200d), new SPointF(x, y), out _);

        Assert.False(result);
    }

    private static void AssertRect(SRectF actual,
        double expectedX,
        double expectedY,
        double expectedWidth,
        double expectedHeight)
    {
        Assert.Equal(expectedX, actual.X, precision: 10);
        Assert.Equal(expectedY, actual.Y, precision: 10);
        Assert.Equal(expectedWidth, actual.Width, precision: 10);
        Assert.Equal(expectedHeight, actual.Height, precision: 10);
    }
}
