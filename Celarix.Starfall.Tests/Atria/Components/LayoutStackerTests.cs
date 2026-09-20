using Celarix.Starfall.Layout.Atria.Components;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Tests.Atria.Components;

public sealed class LayoutStackerTests
{
    [Fact]
    public void Place_VerticalStack_AdvancesAlongYAxisWithMargin()
    {
        var stacker = new LayoutStacker(Direction.Vertical, 10d, 2d);

        var first = stacker.Place(new SSizeF(20d, 30d), 5d, 1);
        var second = stacker.Place(new SSizeF(10d, 15d), 7d, 2);

        AssertRect(first, 5d, 0d, 20d, 30d);
        AssertRect(second, 7d, 50d, 10d, 15d);
    }

    [Fact]
    public void Place_HorizontalStack_PreservesWidthAndHeightAndAdvancesAlongXAxis()
    {
        var stacker = new LayoutStacker(Direction.Horizontal, 10d, 2d);

        var first = stacker.Place(new SSizeF(20d, 30d), 7d, 1);
        var second = stacker.Place(new SSizeF(15d, 10d), 9d, 2);

        AssertRect(first, 0d, 7d, 20d, 30d);
        AssertRect(second, 40d, 9d, 15d, 10d);
    }

    [Theory]
    [InlineData(Direction.Vertical, 140d, 0d, 20d, 30d)]
    [InlineData(Direction.Horizontal, 0d, 135d, 20d, 30d)]
    public void Place_WithCenterAlignment_AlignsOnMinorAxis(Direction direction,
        double expectedX,
        double expectedY,
        double expectedWidth,
        double expectedHeight)
    {
        var stacker = new LayoutStacker(direction, 10d, 2d);

        var result = stacker.Place(new SSizeF(20d, 30d), 100d, 200d, Alignment.Center, 1);

        AssertRect(result, expectedX, expectedY, expectedWidth, expectedHeight);
    }

    [Fact]
    public void PlaceAtSameMajorPosition_UsesLargestMajorSizeBeforeMargin()
    {
        var stacker = new LayoutStacker(Direction.Horizontal, 10d, 2d);
        var objects = new[]
        {
            (new SSizeF(20d, 30d), 4d),
            (new SSizeF(35d, 15d), 8d)
        };

        var results = stacker.PlaceAtSameMajorPosition(objects, 1);
        var next = stacker.Place(new SSizeF(5d, 5d), 12d, 1);

        AssertRect(results[0], 0d, 4d, 20d, 30d);
        AssertRect(results[1], 0d, 8d, 35d, 15d);
        AssertRect(next, 55d, 12d, 5d, 5d);
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
