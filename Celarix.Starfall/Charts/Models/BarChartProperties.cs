using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Rendering.Models;
using ExtendedNumerics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed class BarChartProperties : ChartPropertyBase
{
    private BigDecimal _xMinimum;
    private BigDecimal _xMaximum;
    private double _yMinimum;
    private double _yMaximum;

    private double _barWidthRatioOfSlotWidth;
    private Func<double, SColor> _barColorFormatter;
    private ChartInsets _plotInsets;

    public BigDecimal XMinimum
    {
        get => _xMinimum;
        set => SetProperty(value, _xMinimum, v => _xMinimum = v);
    }

    public BigDecimal XMaximum
    {
        get => _xMaximum;
        set => SetProperty(value, _xMaximum, v => _xMaximum = v);
    }

    public double YMinimum
    {
        get => _yMinimum;
        set => SetProperty(value, _yMinimum, v => _yMinimum = v);
    }

    public double YMaximum
    {
        get => _yMaximum;
        set => SetProperty(value, _yMaximum, v => _yMaximum = v);
    }

    public double BarWidthRatioOfSlotWidth
    {
        get => _barWidthRatioOfSlotWidth;
        set => SetProperty(value, _barWidthRatioOfSlotWidth, v => _barWidthRatioOfSlotWidth = v);
    }

    public Func<double, SColor> BarColorFormatter
    {
        get => _barColorFormatter;
        set => SetProperty(value, _barColorFormatter, v => _barColorFormatter = v);
    }

    /// <summary>
    /// Gets or sets the pixel insets applied inside the display area before laying out the plot and its axes.
    /// Insets that cannot fit are reduced proportionally, so the plot never has negative dimensions.
    /// </summary>
    public ChartInsets PlotInsets
    {
        get => _plotInsets;
        set => SetProperty(value, _plotInsets, v => _plotInsets = v);
    }

    public XRange XRange => new(BigDecimal.Floor(XMinimum).WholeValue, BigDecimal.Ceiling(XMaximum).WholeValue);

    public BarChartProperties(
        BigDecimal xMinimum,
        BigDecimal xMaximum,
        double yMinimum,
        double yMaximum,
        double barWidthRatioOfSlotWidth,
        Func<double, SColor> barColorFormatter,
        ChartInsets plotInsets = default
    )
    {
        _xMinimum = xMinimum;
        _xMaximum = xMaximum;
        _yMinimum = yMinimum;
        _yMaximum = yMaximum;
        _barWidthRatioOfSlotWidth = barWidthRatioOfSlotWidth;
        _barColorFormatter = barColorFormatter;
        _plotInsets = plotInsets;

        if (!Valid(out var ex))
        {
            throw ex;
        }
    }

    protected override bool Valid([NotNullWhen(false)] out Exception? ex)
    {
        if (_xMaximum <= _xMinimum)
        {
            ex = new InvalidOperationException("XMinimum cannot be greater than XMaximum.");
            return false;
        }

        if (_yMaximum <= _yMinimum)
        {
            ex = new InvalidOperationException("YMinimum cannot be greater than YMaximum.");
            return false;
        }

        if (_barWidthRatioOfSlotWidth <= 0d || _barWidthRatioOfSlotWidth > 1d)
        {
            ex = new InvalidOperationException("BarWidthRatioOfSlotWidth must be between 0 and 1.");
            return false;
        }

        if (_barColorFormatter == null)
        {
            ex = new InvalidOperationException("BarColorFormatter cannot be null.");
            return false;
        }

        ex = null;
        return true;
    }
}
