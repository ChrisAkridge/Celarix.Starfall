using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public abstract class ChartProperties
{
    public event EventHandler? PropertiesChanged;

    // Side panel visibilities
    private AnimatedVisiblity _titleVisibility;
    private AnimatedVisiblity _infoPanelVisibility;
    private double? _titleVisibilityToggleProgress;
    private double? _infoPanelVisibilityToggleProgress;

    // Title bar properties
    private double _titleBarHeightRatioOfElement;
    private ChartText _titleText;
    private SFont _titleFont;
    private SColor _titleColor;

    // Info panel properties
    private double _infoPanelWidthRatioOfElement;
    private SColor _infoPanelBorderColor;
    private double _infoPanelBorderThickness;
    private double _infoPanelPaddingRatio;
    private SColor _infoPanelBackgroundColor;
    private SFont _infoPanelBaseFont;
    private double _infoPanelFontSizeMultiplierStep;
    private SColor _infoPanelLabelColor;
    private SColor _infoPanelValueColor;
    private SColor _infoPanelSecondaryColor;
    private InfoPanelDisplays _visibleDisplays;
    private List<double> _displayedPercentiles;

    public AnimatedVisiblity TitleVisibility => _titleVisibility;
    public AnimatedVisiblity InfoPanelVisibility => _infoPanelVisibility;
    public double? TitleVisibilityToggleProgress => _titleVisibilityToggleProgress;
    public double? InfoPanelVisibilityToggleProgress => _infoPanelVisibilityToggleProgress;

    public double TitleBarHeightRatioOfElement
    {
        get => _titleBarHeightRatioOfElement;
        set => SetProperty(value, _titleBarHeightRatioOfElement, v => _titleBarHeightRatioOfElement = v);
    }

    public ChartText TitleText
    {
        get => _titleText;
        set => SetProperty(value, _titleText, v => _titleText = v);
    }

    public SFont TitleFont
    {
        get => _titleFont;
        set => SetProperty(value, _titleFont, v => _titleFont = v);
    }

    public SColor TitleColor
    {
        get => _titleColor;
        set => SetProperty(value, _titleColor, v => _titleColor = v);
    }

    public double InfoPanelWidthRatioOfElement
    {
        get => _infoPanelWidthRatioOfElement;
        set => SetProperty(value, _infoPanelWidthRatioOfElement, v => _infoPanelWidthRatioOfElement = v);
    }

    public SColor InfoPanelBorderColor
    {
        get => _infoPanelBorderColor;
        set => SetProperty(value, _infoPanelBorderColor, v => _infoPanelBorderColor = v);
    }

    public double InfoPanelBorderThickness
    {
        get => _infoPanelBorderThickness;
        set => SetProperty(value, _infoPanelBorderThickness, v => _infoPanelBorderThickness = v);
    }

    public double InfoPanelPaddingRatio
    {
        get => _infoPanelPaddingRatio;
        set => SetProperty(value, _infoPanelPaddingRatio, v => _infoPanelPaddingRatio = v);
    }

    public SColor InfoPanelBackgroundColor
    {
        get => _infoPanelBackgroundColor;
        set => SetProperty(value, _infoPanelBackgroundColor, v => _infoPanelBackgroundColor = v);
    }

    public SFont InfoPanelBaseFont
    {
        get => _infoPanelBaseFont;
        set => SetProperty(value, _infoPanelBaseFont, v => _infoPanelBaseFont = v);
    }

    public double InfoPanelFontSizeMultiplierStep
    {
        get => _infoPanelFontSizeMultiplierStep;
        set => SetProperty(value, _infoPanelFontSizeMultiplierStep, v => _infoPanelFontSizeMultiplierStep = v);
    }

    public SColor InfoPanelLabelColor
    {
        get => _infoPanelLabelColor;
        set => SetProperty(value, _infoPanelLabelColor, v => _infoPanelLabelColor = v);
    }

    public SColor InfoPanelValueColor
    {
        get => _infoPanelValueColor;
        set => SetProperty(value, _infoPanelValueColor, v => _infoPanelValueColor = v);
    }

    public SColor InfoPanelSecondaryColor
    {
        get => _infoPanelSecondaryColor;
        set => SetProperty(value, _infoPanelSecondaryColor, v => _infoPanelSecondaryColor = v);
    }

    public InfoPanelDisplays VisibleDisplays
    {
        get => _visibleDisplays;
        set => SetProperty(value, _visibleDisplays, v => _visibleDisplays = v);
    }

    public IReadOnlyList<double> DisplayedPercentiles => _displayedPercentiles;

    protected void SetProperty<T>(T newValue, T currentValue, Action<T> setter)
    {
        setter(newValue);
        if (!Valid(out var ex))
        {
            setter(currentValue);
            throw ex;
        }
        
        if (!EqualityComparer<T>.Default.Equals(newValue, currentValue))
        {
            PropertiesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual bool Valid([NotNullWhen(false)] out Exception? ex)
    {
        ex = null;
        return true;
    }
}
