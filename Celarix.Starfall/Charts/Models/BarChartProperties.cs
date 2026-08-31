using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed class BarChartProperties : ChartPropertyBase
{
    private BigInteger _xMinimum;
    private BigInteger _xMaximum;
    private double _yMinimum;
    private double _yMaximum;

    private double _barWidthRatioOfSlotWidth;
    private Func<double, SColor> _barColorFormatter;

    public BigInteger XMinimum
    {
        get => _xMinimum;
        set => SetProperty(value, _xMaximum, v => _xMinimum = v);
    }

    public BigInteger XMaximum
    {
        get => _xMaximum;
        set => SetProperty(value, _xMaximum, v => _xMaximum = v);
    }

    public double YMinimum
    {
        get => _yMinimum;
        set => SetProperty(value, _yMaximum, v => _yMinimum = v);
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

    public XRange XRange => new(XMinimum, XMaximum);

    public BarChartProperties(
        BigInteger xMinimum,
        BigInteger xMaximum,
        double yMinimum,
        double yMaximum,
        double barWidthRatioOfSlotWidth,
        Func<double, SColor> barColorFormatter
    )
    {
        _xMinimum = xMinimum;
        _xMaximum = xMaximum;
        _yMinimum = yMinimum;
        _yMaximum = yMaximum;
        _barWidthRatioOfSlotWidth = barWidthRatioOfSlotWidth;
        _barColorFormatter = barColorFormatter;

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
