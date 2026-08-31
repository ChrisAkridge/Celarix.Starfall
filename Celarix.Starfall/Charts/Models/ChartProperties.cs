using Celarix.Starfall.Extensions;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed class ChartProperties : ChartPropertyBase
{
    private const double AnimationDurationSeconds = 0.5;

    // Side panel visibilities
    private AnimatedVisiblity _titleVisibility;
    private AnimatedVisiblity _infoPanelVisibility;
    private double? _titleVisibilityToggleProgress;
    private double? _infoPanelVisibilityToggleProgress;
    private AnimationSlot? _titleVisibilityAnimation;
    private AnimationSlot? _infoPanelVisibilityAnimation;

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
    private InfoPanelSummaries _visibleDisplays;
    private double _infoPanelSummaryItemMargin;
    private List<double> _displayedPercentiles;

    /// <summary>
    /// Gets the visibility state of the chart title bar.
    /// </summary>
    public AnimatedVisiblity TitleVisibility => _titleVisibility;

    /// <summary>
    /// Gets the visibility state of the chart info panel.
    /// </summary>
    public AnimatedVisiblity InfoPanelVisibility => _infoPanelVisibility;

    /// <summary>
    /// Gets the progress of the title visibility toggle animation, if an animation is active, between 0 and 1.
    /// </summary>
    public double? TitleVisibilityToggleProgress => _titleVisibilityToggleProgress;

    /// <summary>
    /// Gets the progress of the info panel visibility toggle animation, if an animation is active, between 0 and 1.
    /// </summary>
    public double? InfoPanelVisibilityToggleProgress => _infoPanelVisibilityToggleProgress;

    /// <summary>
    /// Gets the ratio of the title bar's height to the overall chart element's height.
    /// For example, a value of 0.1 means the title bar occupies 10% of the chart's height.
    /// </summary>
    public double TitleBarHeightRatioOfElement
    {
        get => _titleBarHeightRatioOfElement;
        set => SetProperty(value, _titleBarHeightRatioOfElement, v => _titleBarHeightRatioOfElement = v);
    }

    /// <summary>
    /// Gets or sets the text of the chart title.
    /// </summary>
    public ChartText TitleText
    {
        get => _titleText;
        set => SetProperty(value, _titleText, v => _titleText = v);
    }

    /// <summary>
    /// Gets or sets the font used for the chart title.
    /// </summary>
    public SFont TitleFont
    {
        get => _titleFont;
        set => SetProperty(value, _titleFont, v => _titleFont = v);
    }

    /// <summary>
    /// Gets or sets the color of the chart title text.
    /// </summary>
    public SColor TitleColor
    {
        get => _titleColor;
        set => SetProperty(value, _titleColor, v => _titleColor = v);
    }

    /// <summary>
    /// Gets or sets the ratio of the info panel's width to the overall chart element's width.
    /// For example, a value of 0.2 means the info panel occupies 20% of the chart's width.
    /// </summary>
    public double InfoPanelWidthRatioOfElement
    {
        get => _infoPanelWidthRatioOfElement;
        set => SetProperty(value, _infoPanelWidthRatioOfElement, v => _infoPanelWidthRatioOfElement = v);
    }

    /// <summary>
    /// Gets or sets the color of the border around the info panel.
    /// </summary>
    public SColor InfoPanelBorderColor
    {
        get => _infoPanelBorderColor;
        set => SetProperty(value, _infoPanelBorderColor, v => _infoPanelBorderColor = v);
    }

    /// <summary>
    /// Gets or sets the thickness of the border around the info panel, in pixels.
    /// </summary>
    public double InfoPanelBorderThickness
    {
        get => _infoPanelBorderThickness;
        set => SetProperty(value, _infoPanelBorderThickness, v => _infoPanelBorderThickness = v);
    }

    /// <summary>
    /// Gets or sets the padding ratio for the info panel, which determines the amount of space between the content and the edges of the panel.
    /// For example, a value of 0.1 means a 10% gap is maintained between the content and the border on all four sides,
    /// meaning the content takes up 80% of the panel's width and height. The valid values are between 0 and 0.5, inclusive.
    /// </summary>
    public double InfoPanelPaddingRatio
    {
        get => _infoPanelPaddingRatio;
        set => SetProperty(value, _infoPanelPaddingRatio, v => _infoPanelPaddingRatio = v);
    }

    /// <summary>
    /// Gets or sets the background color of the info panel.
    /// </summary>
    public SColor InfoPanelBackgroundColor
    {
        get => _infoPanelBackgroundColor;
        set => SetProperty(value, _infoPanelBackgroundColor, v => _infoPanelBackgroundColor = v);
    }

    /// <summary>
    /// Gets or sets the base font used for the info panel. This font is used as a reference for scaling text sizes within the panel.
    /// </summary>
    public SFont InfoPanelBaseFont
    {
        get => _infoPanelBaseFont;
        set => SetProperty(value, _infoPanelBaseFont, v => _infoPanelBaseFont = v);
    }

    /// <summary>
    /// Gets or sets a multiplier applied to font size for each step. For example, if the base font size
    /// is 12 and the multiplier is 1.2, then -1 steps is 10, 0 steps is 12, and +1 step is 14.4. Steps
    /// are integer-sized and can be semantically defined. For example, -1 might be "small", -2 might be "tiny",
    /// 0 might be "normal", +1 might be "large", and +2 might be "huge".
    /// </summary>
    public double InfoPanelFontSizeMultiplierStep
    {
        get => _infoPanelFontSizeMultiplierStep;
        set => SetProperty(value, _infoPanelFontSizeMultiplierStep, v => _infoPanelFontSizeMultiplierStep = v);
    }

    /// <summary>
    /// Gets or sets the color of the labels within the info panel.
    /// </summary>
    public SColor InfoPanelLabelColor
    {
        get => _infoPanelLabelColor;
        set => SetProperty(value, _infoPanelLabelColor, v => _infoPanelLabelColor = v);
    }

    /// <summary>
    /// Gets or sets the color of the values within the info panel.
    /// </summary>
    public SColor InfoPanelValueColor
    {
        get => _infoPanelValueColor;
        set => SetProperty(value, _infoPanelValueColor, v => _infoPanelValueColor = v);
    }

    /// <summary>
    /// Gets or sets an extra color for the text of the info panel, where needed.
    /// </summary>
    public SColor InfoPanelSecondaryColor
    {
        get => _infoPanelSecondaryColor;
        set => SetProperty(value, _infoPanelSecondaryColor, v => _infoPanelSecondaryColor = v);
    }

    /// <summary>
    /// Gets or sets a value specifying which statistical displays are visible in the info panel.
    /// </summary>
    public InfoPanelSummaries VisibleDisplays
    {
        get => _visibleDisplays;
        set => SetProperty(value, _visibleDisplays, v => _visibleDisplays = v);
    }

    public double InfoPanelSummaryItemMargin
    {
        get => _infoPanelSummaryItemMargin;
        set => SetProperty(value, _infoPanelSummaryItemMargin, v => _infoPanelSummaryItemMargin = v);
    }

    /// <summary>
    /// Gets a read-only list of percentiles that are displayed in the info panel, if percentiles are enabled.
    /// </summary>
    public IReadOnlyList<double> DisplayedPercentiles => _displayedPercentiles;

    /// <summary>
    /// Gets the current height ratio of the title bar relative to the overall chart element,
    /// taking into account the visibility state and any ongoing animations.
    /// </summary>
    public double CurrentTitleBarHeightRatioOfElement
    {
        get
        {
            if (TitleVisibility == AnimatedVisiblity.Visible)
            {
                return _titleBarHeightRatioOfElement;
            }
            else if (TitleVisibility is AnimatedVisiblity.Appearing or AnimatedVisiblity.Disappearing)
            {
                return _titleBarHeightRatioOfElement * (_titleVisibilityToggleProgress ?? 0.0);
            }
            else
            {
                return 0.0;
            }
        }
    }

    /// <summary>
    /// Gets the current width ratio of the info panel relative to the overall chart element,
    /// </summary>
    public double CurrentInfoPanelWidthRatioOfElement
    {
        get
        {
            if (InfoPanelVisibility == AnimatedVisiblity.Visible)
            {
                return _infoPanelWidthRatioOfElement;
            }
            else if (InfoPanelVisibility is AnimatedVisiblity.Appearing or AnimatedVisiblity.Disappearing)
            {
                return _infoPanelWidthRatioOfElement * (_infoPanelVisibilityToggleProgress ?? 0.0);
            }
            else
            {
                return 0.0;
            }
        }
    }

    public ChartProperties(
        bool startTitleBarVisible,
        bool startInfoPanelVisible,
        double titleBarHeightRatioOfElement,
        ChartText titleText,
        SFont titleFont,
        SColor titleColor,
        double infoPanelWidthRatioOfElement,
        SColor infoPanelBorderColor,
        double infoPanelBorderThickness,
        double infoPanelPaddingRatio,
        SColor infoPanelBackgroundColor,
        SFont infoPanelBaseFont,
        double infoPanelFontSizeMultiplierStep,
        SColor infoPanelLabelColor,
        SColor infoPanelValueColor,
        SColor infoPanelSecondaryColor,
        InfoPanelSummaries visibleDisplays,
        double visibleDisplayItemMargin,
        IEnumerable<double> displayedPercentiles)
    {
        _titleVisibility = startTitleBarVisible ? AnimatedVisiblity.Visible : AnimatedVisiblity.Invisible;
        _infoPanelVisibility = startInfoPanelVisible ? AnimatedVisiblity.Visible : AnimatedVisiblity.Invisible;
        _titleBarHeightRatioOfElement = titleBarHeightRatioOfElement;
        _titleText = titleText;
        _titleFont = titleFont;
        _titleColor = titleColor;
        _infoPanelWidthRatioOfElement = infoPanelWidthRatioOfElement;
        _infoPanelBorderColor = infoPanelBorderColor;
        _infoPanelBorderThickness = infoPanelBorderThickness;
        _infoPanelPaddingRatio = infoPanelPaddingRatio;
        _infoPanelBackgroundColor = infoPanelBackgroundColor;
        _infoPanelBaseFont = infoPanelBaseFont;
        _infoPanelFontSizeMultiplierStep = infoPanelFontSizeMultiplierStep;
        _infoPanelLabelColor = infoPanelLabelColor;
        _infoPanelValueColor = infoPanelValueColor;
        _infoPanelSecondaryColor = infoPanelSecondaryColor;
        _visibleDisplays = visibleDisplays;
        _infoPanelSummaryItemMargin = visibleDisplayItemMargin;
        _displayedPercentiles = [.. displayedPercentiles.OrderBy(p => p)];

        // Since we gate write access to displayedPercentiles, we have to validate the initial value here.
        PercentileListValidOrThrow(_displayedPercentiles);

        if (!Valid(out var ex))
        {
            throw ex;
        }
    }

    private void PercentileListValidOrThrow(IList<double> percentiles)
    {
        if (!_visibleDisplays.HasFlag(InfoPanelSummaries.Percentiles))
        {
            // If percentiles aren't visible, the list is allowed to be empty.
            if (percentiles.Count == 0)
            {
                return;
            }
        }
        else
        {
            // If percentiles are visible, the list must contain at least one value.
            if (percentiles.Count == 0)
            {
                throw new ArgumentException("Percentile list cannot be empty when percentiles are visible.", nameof(percentiles));
            }

            // Validate that all percentiles are between 0 and 100, inclusive.
            if (percentiles.Any(p => p < 0d || p > 100d))
            {
                throw new ArgumentException("All percentiles must be between 0 and 100, inclusive.", nameof(percentiles));
            }

            // Validate that there are no duplicate percentiles.
            if (percentiles.HasDuplicates())
            {
                throw new ArgumentException("Percentile list cannot contain duplicate values.", nameof(percentiles));
            }
        }
    }

    public void SetDisplayedPercentiles(IEnumerable<double> percentiles)
    {
        List<double> percentileList = [.. percentiles.OrderBy(p => p)];
        PercentileListValidOrThrow(percentileList);
        _displayedPercentiles = percentileList;
        OnPropertiesChanged();
    }

    public void AddPercentile(double percentile)
    {
        if (percentile < 0d || percentile > 100d)
        {
            throw new ArgumentOutOfRangeException(nameof(percentile), percentile, "Percentile must be between 0 and 100, inclusive.");
        }
        if (_displayedPercentiles.Contains(percentile))
        {
            throw new ArgumentException("Percentile list cannot contain duplicate values.", nameof(percentile));
        }
        _displayedPercentiles.Add(percentile);
        _displayedPercentiles.Sort();
        OnPropertiesChanged();
    }

    public void SetTitleBarVisibility(bool visible, AnimationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if ((visible && _titleVisibility is AnimatedVisiblity.Visible or AnimatedVisiblity.Appearing)
            || (!visible && _titleVisibility is AnimatedVisiblity.Invisible or AnimatedVisiblity.Disappearing))
        {
            // No change needed.
            return;
        }

        _titleVisibilityAnimation ??= context.CreateSlot("ChartProperties.TitleVisibility");
        var startingProgress = _titleVisibilityToggleProgress
            ?? (_titleVisibility == AnimatedVisiblity.Visible ? 1d : 0d);
        var endingProgress = visible ? 1d : 0d;
        _titleVisibility = visible ? AnimatedVisiblity.Appearing : AnimatedVisiblity.Disappearing;
        _titleVisibilityToggleProgress = startingProgress;
        OnPropertiesChanged();

        var frames = AnimationContext.SecondsToFrames(AnimationDurationSeconds);
        _titleVisibilityAnimation.Replace(() => FixedDurationAnimation.StartNow(frames, progress =>
            {
                _titleVisibilityToggleProgress = startingProgress
                    + ((endingProgress - startingProgress) * progress);
                OnPropertiesChanged();
            }, () =>
            {
                _titleVisibility = visible ? AnimatedVisiblity.Visible : AnimatedVisiblity.Invisible;
                _titleVisibilityToggleProgress = null;
                OnPropertiesChanged();
            }), AnimationSlotReplacementBehavior.CancelExisting);
    }

    public void SetInfoPanelVisibility(bool visible, AnimationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if ((visible && _infoPanelVisibility is AnimatedVisiblity.Visible or AnimatedVisiblity.Appearing)
            || (!visible && _infoPanelVisibility is AnimatedVisiblity.Invisible or AnimatedVisiblity.Disappearing))
        {
            // No change needed.
            return;
        }

        _infoPanelVisibilityAnimation ??= context.CreateSlot("ChartProperties.InfoPanelVisibility");
        var startingProgress = _infoPanelVisibilityToggleProgress
            ?? (_infoPanelVisibility == AnimatedVisiblity.Visible ? 1d : 0d);
        var endingProgress = visible ? 1d : 0d;
        _infoPanelVisibility = visible ? AnimatedVisiblity.Appearing : AnimatedVisiblity.Disappearing;
        _infoPanelVisibilityToggleProgress = startingProgress;
        OnPropertiesChanged();

        var frames = AnimationContext.SecondsToFrames(AnimationDurationSeconds);
        _infoPanelVisibilityAnimation.Replace(() => FixedDurationAnimation.StartNow(frames, progress =>
            {
                _infoPanelVisibilityToggleProgress = startingProgress
                    + ((endingProgress - startingProgress) * progress);
                OnPropertiesChanged();
            }, () =>
            {
                _infoPanelVisibility = visible ? AnimatedVisiblity.Visible : AnimatedVisiblity.Invisible;
                _infoPanelVisibilityToggleProgress = null;
                OnPropertiesChanged();
            }), AnimationSlotReplacementBehavior.CancelExisting);
    }

    protected override bool Valid([NotNullWhen(false)] out Exception? ex)
    {
        if (_titleBarHeightRatioOfElement < 0d || _titleBarHeightRatioOfElement > 1d)
        {
            ex = new ArgumentOutOfRangeException(nameof(TitleBarHeightRatioOfElement), _titleBarHeightRatioOfElement, "Value must be between 0 and 1.");
            return false;
        }

        if (_titleFont == null)
        {
            ex = new ArgumentNullException(nameof(TitleFont));
            return false;
        }

        if (_infoPanelWidthRatioOfElement < 0d || _infoPanelWidthRatioOfElement > 1d)
        {
            ex = new ArgumentOutOfRangeException(nameof(InfoPanelWidthRatioOfElement), _infoPanelWidthRatioOfElement, "Value must be between 0 and 1.");
            return false;
        }

        if (_infoPanelBorderThickness < 0d)
        {
            ex = new ArgumentOutOfRangeException(nameof(InfoPanelBorderThickness), _infoPanelBorderThickness, "Value must be non-negative.");
            return false;
        }

        if (_infoPanelPaddingRatio < 0d || _infoPanelPaddingRatio > 0.5d)
        {
            ex = new ArgumentOutOfRangeException(nameof(InfoPanelPaddingRatio), _infoPanelPaddingRatio, "Value must be between 0 and 0.5.");
            return false;
        }

        if (_infoPanelBaseFont == null)
        {
            ex = new ArgumentNullException(nameof(InfoPanelBaseFont));
            return false;
        }

        if (_infoPanelFontSizeMultiplierStep <= 0d)
        {
            ex = new ArgumentOutOfRangeException(nameof(InfoPanelFontSizeMultiplierStep), _infoPanelFontSizeMultiplierStep, "Value must be greater than 0.");
            return false;
        }

        if (!Enum.IsDefined(_visibleDisplays))
        {
            ex = new ArgumentOutOfRangeException(nameof(VisibleDisplays), _visibleDisplays, "Value must be a valid InfoPanelDisplays enum value.");
            return false;
        }

        if (_infoPanelSummaryItemMargin < 0d)
        {
            ex = new ArgumentOutOfRangeException(nameof(InfoPanelSummaryItemMargin), _infoPanelSummaryItemMargin, "Value must be non-negative.");
            return false;
        }

        ex = null;
        return true;
    }
}
