using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

/// <summary>
/// A class that encapsulates the properties of an axis in a chart.
/// </summary>
/// <typeparam name="T">The numeric type of the axis values.</typeparam>
public sealed class AxisProperties<T> : ChartPropertyBase
    where T : struct, INumber<T>
{
    private double _sizeRatioOfParent;

    private T _lowestValidMinimum;
    private T _highestValidMinimum;
    private T _lowestValidMaximum;
    private T _highestValidMaximum;

    private T _minimum;
    private T _maximum;

    private GridlineStyle _gridlineStyle;
    private T _gridlineGap;
    private double _gridlineThickness;
    private SColor _gridlineColor;

    private SFont _labelFont = new SFontFamily("Calibri", 12f);
    private SColor _labelColor;
    private SAngle _labelAngle;
    private double _labelMarginEm;

    private Func<T, ChartText> _tickFormatter = v => ChartText.String(v.ToString() ?? string.Empty);

    /// <summary>
    /// Gets or sets the size of the axis relative to its parent container.
    /// Whether this describes the width or height depends on whether the parent draws this axis horizontally or vertically.
    /// </summary>
    public double SizeRatioOfParent
    {
        get => _sizeRatioOfParent;
        set => SetProperty(value, _sizeRatioOfParent, v => _sizeRatioOfParent = v);
    }

    /// <summary>
    /// Gets or sets the lowest valid value for the <see cref="Minimum"/> property.
    /// </summary>
    public T LowestValidMinimum
    {
        get => _lowestValidMinimum;
        set => SetProperty(value, _lowestValidMinimum, v => _lowestValidMinimum = v);
    }

    /// <summary>
    /// Gets or sets the highest valid value for the <see cref="Minimum"/> property.
    /// </summary>
    public T HighestValidMinimum
    {
        get => _highestValidMinimum;
        set => SetProperty(value, _highestValidMinimum, v => _highestValidMinimum = v);
    }

    /// <summary>
    /// Gets or sets the minimum value of the axis.
    /// </summary>
    public T Minimum
    {
        get => _minimum;
        set => SetProperty(value, _minimum, v => _minimum = v);
    }

    /// <summary>
    /// Gets or sets the lowest valid value for the <see cref="Maximum"/> property.
    /// </summary>
    public T LowestValidMaximum
    {
        get => _lowestValidMaximum;
        set => SetProperty(value, _lowestValidMaximum, v => _lowestValidMaximum = v);
    }

    /// <summary>
    /// Gets or sets the highest valid value for the <see cref="Maximum"/> property.
    /// </summary>
    public T HighestValidMaximum
    {
        get => _highestValidMaximum;
        set => SetProperty(value, _highestValidMaximum, v => _highestValidMaximum = v);
    }

    /// <summary>
    /// Gets or sets the maximum value of the axis.
    /// </summary>
    public T Maximum
    {
        get => _maximum;
        set => SetProperty(value, _maximum, v => _maximum = v);
    }

    /// <summary>
    /// Gets or sets the style of the gridlines on the axis.
    /// </summary>
    public GridlineStyle GridlineStyle
    {
        get => _gridlineStyle;
        set => SetProperty(value, _gridlineStyle, v => _gridlineStyle = v);
    }
    
    /// <summary>
    /// Gets or sets the thickness of the gridlines on the axis in pixels.
    /// </summary>
    public double GridlineThickness
    {
        get => _gridlineThickness;
        set => SetProperty(value, _gridlineThickness, v => _gridlineThickness = v);
    }

    /// <summary>
    /// Gets or sets the color of the gridlines on the axis.
    /// </summary>
    public SColor GridlineColor
    {
        get => _gridlineColor;
        set => SetProperty(value, _gridlineColor, v => _gridlineColor = v);
    }

    /// <summary>
    /// Gets or sets the gap between gridlines on the axis, in units of the axis values.
    /// For example, gaps might be 500 units apart on a numeric axis, or 6 hours apart on a time axis.
    /// </summary>
    public T GridlineGap
    {
        get => _gridlineGap;
        set => SetProperty(value, _gridlineGap, v => _gridlineGap = v);
    }

    /// <summary>
    /// Gets or sets the font used for the axis labels.
    /// </summary>
    public SFont LabelFont
    {
        get => _labelFont;
        set => SetProperty(value, _labelFont, v => _labelFont = v);
    }

    /// <summary>
    /// Gets or sets the color of the axis labels.
    /// </summary>
    public SColor LabelColor
    {
        get => _labelColor;
        set => SetProperty(value, _labelColor, v => _labelColor = v);
    }

    /// <summary>
    /// Gets or sets the angle of the axis labels in degrees. A value of 0 means horizontal, and a value of 90 means vertical,
    /// top to bottom.
    /// </summary>
    public SAngle LabelAngle
    {
        get => _labelAngle;
        set => SetProperty(value, _labelAngle, v => _labelAngle = v);
    }

    /// <summary>
    /// Gets or sets the margin between the axis labels and the axis itself, in em units. An em unit is equal to the current font size.
    /// </summary>
    public double LabelMarginEm
    {
        get => _labelMarginEm;
        set => SetProperty(value, _labelMarginEm, v => _labelMarginEm = v);
    }

    /// <summary>
    /// Gets or sets the function used to convert numeric values to tick labels.
    /// </summary>
    public Func<T, ChartText> TickFormatter
    {
        get => _tickFormatter;
        set => SetProperty(value, _tickFormatter, v => _tickFormatter = v);
    }

    public AxisProperties(T minimum, T maximum, GridlineStyle gridlineStyle, double gridlineThickness, SColor gridlineColor,
        T gridlineGap, SFont labelFont, SColor labelColor, SAngle labelAngle, double labelMarginEm, Func<T, ChartText> tickFormatter)
    {
        _minimum = minimum;
        _lowestValidMinimum = minimum;
        _highestValidMinimum = minimum;

        _maximum = maximum;
        _lowestValidMaximum = maximum;
        _highestValidMaximum = maximum;

        _gridlineStyle = gridlineStyle;
        _gridlineThickness = gridlineThickness;
        _gridlineColor = gridlineColor;
        _gridlineGap = gridlineGap;

        _labelFont = labelFont;
        _labelColor = labelColor;
        _labelAngle = labelAngle;
        _labelMarginEm = labelMarginEm;

        _tickFormatter = tickFormatter;

        if (!Valid(out var ex))
        {
            throw ex;
        }
    }

    protected override bool Valid([System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out Exception? ex)
    {
        var baseValid = base.Valid(out ex!);
        if (!baseValid)
        {
            return false;
        }

        if (_sizeRatioOfParent < 0 || _sizeRatioOfParent > 1)
        {
            ex = new InvalidOperationException("Size ratio of parent must be between 0 and 1.");
            return false;
        }

        if (_lowestValidMinimum.CompareTo(_highestValidMinimum) > 0)
        {
            ex = new InvalidOperationException("Lowest valid minimum cannot be greater than highest valid minimum.");
            return false;
        }
        if (_lowestValidMaximum.CompareTo(_highestValidMaximum) > 0)
        {
            ex = new InvalidOperationException("Lowest valid maximum cannot be greater than highest valid maximum.");
            return false;
        }
        if (_minimum.CompareTo(_maximum) > 0)
        {
            ex = new InvalidOperationException("Minimum cannot be greater than maximum.");
            return false;
        }
        if (_minimum.CompareTo(_lowestValidMinimum) < 0 || _minimum.CompareTo(_highestValidMinimum) > 0)
        {
            ex = new InvalidOperationException("Minimum is out of valid range.");
            return false;
        }
        if (_maximum.CompareTo(_lowestValidMaximum) < 0 || _maximum.CompareTo(_highestValidMaximum) > 0)
        {
            ex = new InvalidOperationException("Maximum is out of valid range.");
            return false;
        }

        if (!Enum.IsDefined(_gridlineStyle))
        {
            ex = new InvalidOperationException("Gridline style is not a valid value.");
            return false;
        }

        if (_gridlineThickness < 0)
        {
            ex = new InvalidOperationException("Gridline thickness cannot be negative.");
            return false;
        }

        if (_gridlineGap.CompareTo(T.Zero) <= 0)
        {
            ex = new InvalidOperationException("Gridline gap must be greater than zero.");
            return false;
        }

        if (_labelFont == null)
        {
            ex = new InvalidOperationException("Label font cannot be null.");
            return false;
        }

        if (_labelAngle.Degrees < 0 || _labelAngle.Degrees > 360)
        {
            ex = new InvalidOperationException("Label angle must be between 0 and 360 degrees.");
            return false;
        }

        if (_labelMarginEm < 0)
        {
            ex = new InvalidOperationException("Label margin cannot be negative.");
            return false;
        }

        if (_tickFormatter == null)
        {
            ex = new InvalidOperationException("Tick formatter cannot be null.");
            return false;
        }

        ex = null;
        return true;
    }
}
