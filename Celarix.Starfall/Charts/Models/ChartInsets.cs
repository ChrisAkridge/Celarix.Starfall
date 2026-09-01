using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Charts.Models;

/// <summary>
/// Pixel insets applied to the four sides of a chart region.
/// </summary>
public readonly record struct ChartInsets
{
    public static ChartInsets Zero => default;

    public double Left { get; }
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }

    public ChartInsets(double left, double top, double right, double bottom)
    {
        Validate(left, nameof(left));
        Validate(top, nameof(top));
        Validate(right, nameof(right));
        Validate(bottom, nameof(bottom));

        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public static ChartInsets Uniform(double amount) => new(amount, amount, amount, amount);

    /// <summary>
    /// Insets a rectangle without producing negative dimensions. If opposing insets exceed the available
    /// dimension, they are reduced proportionally and the resulting dimension is zero.
    /// </summary>
    public SRectF ApplyTo(SRectF bounds)
    {
        var availableWidth = Math.Max(0d, bounds.Width);
        var availableHeight = Math.Max(0d, bounds.Height);
        var horizontalScale = GetScale(Left + Right, availableWidth);
        var verticalScale = GetScale(Top + Bottom, availableHeight);
        var appliedLeft = Left * horizontalScale;
        var appliedTop = Top * verticalScale;
        var appliedRight = Right * horizontalScale;
        var appliedBottom = Bottom * verticalScale;

        return new SRectF(
            bounds.X + appliedLeft,
            bounds.Y + appliedTop,
            Math.Max(0d, availableWidth - appliedLeft - appliedRight),
            Math.Max(0d, availableHeight - appliedTop - appliedBottom));
    }

    private static double GetScale(double requested, double available) =>
        requested > available && requested > 0d ? available / requested : 1d;

    private static void Validate(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Chart insets must be finite and non-negative.");
        }
    }
}
