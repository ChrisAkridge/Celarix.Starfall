using Celarix.Starfall.Charts;
using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System.Numerics;

namespace Celarix.Starfall.Tests.Charts;

public sealed class IntegralAxisLabelLayoutTests
{
    [Fact]
    public void PanRetainsStrideAndMaterializedLibraLayouts()
    {
        var materializations = 0;
        var layout = new IntegralAxisLabelLayout();
        LibraLayoutResult Factory(BigInteger value)
        {
            materializations++;
            return Label(width: 10);
        }
        SRectF Slot(BigInteger value) => new((double)value * 20d, 0, 0, 0);

        var first = layout.Update(new XRange(0, 4), Factory, Slot, Side.Bottom, 0, 1.2, true);
        var retainedLayout = first.Single(label => label.Value == 2).LibraLayoutResult;
        var stride = layout.Stride;
        var firstMaterializationCount = materializations;

        var second = layout.Update(new XRange(1, 5), Factory, Slot, Side.Bottom, 0, 1.2, false);

        Assert.Equal(stride, layout.Stride);
        Assert.Same(retainedLayout, second.Single(label => label.Value == 2).LibraLayoutResult);
        Assert.Equal(firstMaterializationCount + 1, materializations);
    }

    [Fact]
    public void WideEnteringLabelIsSuppressedWithoutReplacingRetainedLabels()
    {
        var layout = new IntegralAxisLabelLayout();
        LibraLayoutResult Factory(BigInteger value) => Label(value == 6 ? 100 : 10);
        SRectF Slot(BigInteger value) => new((double)value * 20d, 0, 0, 0);

        var first = layout.Update(new XRange(0, 4), Factory, Slot, Side.Bottom, 0, 1, true);
        var retainedValues = first.Select(label => label.Value).ToHashSet();
        var second = layout.Update(new XRange(1, 5), Factory, Slot, Side.Bottom, 0, 1, false);

        Assert.DoesNotContain(second, label => label.Value == 6);
        Assert.All(second.Where(label => retainedValues.Contains(label.Value)),
            label => Assert.Contains(label.Value, retainedValues));
    }

    [Fact]
    public void DensityRecalculationUsesARegularZeroAnchoredStride()
    {
        var layout = new IntegralAxisLabelLayout();
        LibraLayoutResult Factory(BigInteger _) => Label(width: 30);
        SRectF Slot(BigInteger value) => new((double)value * 10d, 0, 0, 0);

        var labels = layout.Update(new XRange(-5, 5), Factory, Slot, Side.Bottom, 0, 1, true);

        var stride = Assert.IsType<BigInteger>(layout.Stride);
        Assert.True(stride > 1);
        Assert.All(labels, label => Assert.Equal(BigInteger.Zero, label.Value % stride));
    }

    private static LibraLayoutResult Label(double width) => new(
        Array.Empty<LibraRenderable>(), new SRectF(0, 0, width, 4), 0, 0);
}
