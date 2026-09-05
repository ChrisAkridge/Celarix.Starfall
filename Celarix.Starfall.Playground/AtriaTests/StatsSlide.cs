using Celarix.Starfall.Charts;
using Celarix.Starfall.Charts.Displays;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests;

internal sealed class StatsSlide : AtriaSlide
{
    private BarChartDisplay? _barChart;
    private ChartProperties? _chartProperties;
    private ChartElement? _chartElement;

    public StatsSlide(int width, int height) : base(width, height)
    {
    }

    public override void Initialize()
    {
        BackgroundColor = new SColor(8, 0, 130, 255);

        var dates = new DateOnly[]
        {
            DateOnly.Parse("7/12/2026"),
            DateOnly.Parse("7/13/2026"),
            DateOnly.Parse("7/14/2026"),
            DateOnly.Parse("7/15/2026"),
            DateOnly.Parse("7/16/2026"),
            DateOnly.Parse("7/17/2026"),
            DateOnly.Parse("7/18/2026"),
            DateOnly.Parse("7/19/2026"),
            DateOnly.Parse("7/20/2026"),
            DateOnly.Parse("7/21/2026"),
            DateOnly.Parse("7/22/2026"),
            DateOnly.Parse("7/23/2026"),
            DateOnly.Parse("7/24/2026"),
            DateOnly.Parse("7/25/2026"),
            DateOnly.Parse("7/26/2026"),
            DateOnly.Parse("7/27/2026"),
            DateOnly.Parse("7/28/2026"),
            DateOnly.Parse("7/29/2026"),
            DateOnly.Parse("7/30/2026"),
            DateOnly.Parse("7/31/2026"),
            DateOnly.Parse("8/1/2026"),
            DateOnly.Parse("8/2/2026"),
            DateOnly.Parse("8/3/2026"),
            DateOnly.Parse("8/4/2026"),
            DateOnly.Parse("8/5/2026"),
            DateOnly.Parse("8/6/2026"),
            DateOnly.Parse("8/7/2026"),
            DateOnly.Parse("8/8/2026"),
            DateOnly.Parse("8/9/2026"),
            DateOnly.Parse("8/10/2026"),
            DateOnly.Parse("8/11/2026"),
            DateOnly.Parse("8/12/2026"),
            DateOnly.Parse("8/13/2026"),
            DateOnly.Parse("8/14/2026"),
            DateOnly.Parse("8/15/2026"),
            DateOnly.Parse("8/16/2026"),
            DateOnly.Parse("8/17/2026"),
            DateOnly.Parse("8/18/2026"),
            DateOnly.Parse("8/19/2026"),
            DateOnly.Parse("8/20/2026"),
            DateOnly.Parse("8/21/2026"),
            DateOnly.Parse("8/22/2026"),
            DateOnly.Parse("8/23/2026")
        };

        var calories = new int[]
        {
            2350,
            2350,
            2350,
            2350,
            2350,
            2350,
            2530,
            2576,
            2621,
            2577,
            2345,
            2500,
            2870,
            3365,
            3539,
            2405,
            3223,
            2484,
            2615,
            3725,
            3380,
            4184,
            2332,
            2512,
            2515,
            2257,
            3130,
            2510,
            2410,
            2710,
            3015,
            4440,
            3075,
            3192,
            2440,
            3056,
            3495,
            3490,
            3320,
            2505,
            2890,
            2530,
            2530
        };

        var data = new List<(DateOnly, int)>();
        for (var i = 0; i < dates.Length; i++)
        {
            data.Add((dates[i], calories[i]));
        }

        var labelFont = new SFontFamily("Calibri", 12);
        var yAxisColor = new SColor(128, 128, 128, 255);
        var dataSeries = new DataSeries(data.Select(d => new DataSeriesPoint(
            (BigInteger)(d.Item1.DayNumber - DateOnly.Parse("7/12/2026").DayNumber),
            (double)d.Item2
        )));
        var infoPanelProperties = new InfoPanelProviderProperties<double, BigInteger>(
            TickFormatters.NaNAsDash(d => $"{d:0.##} Cal"),
            y => double.IsNaN(y) ? new ChartText("-") : AlternateDataFormat(y),
            x => new ChartText(DateOnly.Parse("7/12/2026").AddDays((int)x).ToString("M/d")),
            null
        );
        var barChartProperties = new BarChartProperties(
            xMinimum: 0,
            xMaximum: 42,
            yMinimum: 0,
            yMaximum: 5000,
            barWidthRatioOfSlotWidth: 0.8d,
            barColorFormatter: y => new SColor(128, 128, 128, 255),
            plotInsets: ChartInsets.Uniform(12d)
        );
        var xAxisProperties = new AxisProperties<BigInteger>(0.1d, 0, 42, GridlineStyle.Tick, 1d, SColor.White, BigInteger.One, labelFont, SColor.White, SAngle.Zero,
            0.5d, x => new ChartText(DateOnly.Parse("7/12/2026").AddDays((int)x).ToString("M/d")),
            labelFitExtentMultiplier: 1.2d
        );
        var yAxisProperties = new AxisProperties<double>(0.1d, 0, 5000, GridlineStyle.Gridline, 1d, yAxisColor, 500d, labelFont, yAxisColor, SAngle.Zero,
            0.5d, y => new ChartText(y.ToString()),
            labelFitExtentMultiplier: 1.2d
        );
        var chartProperties = new ChartProperties(
            startTitleBarVisible: true,
            startInfoPanelVisible: true,
            titleBarHeightRatioOfElement: 0.1d,
            titleText: new ChartText("Calories Consumed"),
            titleFont: new SFontFamily("Calibri", 16),
            titleColor: SColor.White,
            infoPanelWidthRatioOfElement: 0.2d,
            infoPanelBorderColor: new SColor(120, 145, 220, 150),
            infoPanelBorderThickness: 1d,
            infoPanelPaddingRatio: 0.08d,
            infoPanelBackgroundColor: new SColor(15, 18, 55, 235),
            infoPanelBaseFont: new SFontFamily("Calibri", 12),
            infoPanelFontSizeMultiplierStep: 1.8d,
            infoPanelLabelColor: new SColor(185, 195, 225, 255),
            infoPanelValueColor: SColor.Yellow,
            infoPanelSecondaryColor: new SColor(120, 190, 255, 255),
            visibleDisplays: InfoPanelSummaries.All,
            visibleDisplayItemMargin: 0.1d,
            displayedPercentiles: [1m, 5m, 10m, 25m, 50m, 75m, 90m, 95m, 99m]
        );

        var barChart = new BarChartDisplay(MeasurementService, dataSeries, barChartProperties, xAxisProperties, yAxisProperties, infoPanelProperties);
        var chartElement = new ChartElement(chartProperties, barChart, "#chart")
        {
            Size = new SSizeF(1100, 700),
            Opacity = 1d
        };
        var chartAnchor = new BasisPoint(Center, "#chartAnchor");
        chartElement.AnchorCenterTo(chartAnchor);
        Add([chartElement, chartAnchor]);

        _barChart = barChart;
        _chartProperties = chartProperties;
        _chartElement = chartElement;
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
    }

    public override void KeyUp(SKeyboardEvent keyboardEvent)
    {
        if (keyboardEvent.Key == SKey.Left)
        {
            _barChart?.AnimateScrollToXRange(_barChart.Properties.XMinimum - 7, _barChart.Properties.XMaximum - 7, 1d);
        }
        else if (keyboardEvent.Key == SKey.Right)
        {
            _barChart?.AnimateScrollToXRange(_barChart.Properties.XMinimum + 7, _barChart.Properties.XMaximum + 7, 1d);
        }
        else if (keyboardEvent.Key == SKey.Up)
        {
            _chartElement?.SetTitleBarVisibility(_chartProperties?.TitleVisibility != AnimatedVisiblity.Visible);
        }
        else if (keyboardEvent.Key == SKey.Down)
        {
            _chartElement?.SetInfoPanelVisibility(_chartProperties?.InfoPanelVisibility != AnimatedVisiblity.Visible);
        }
        else if (keyboardEvent.Key == SKey.Q)
        {
            _barChart?.Reveal(0.75d, Easings.Smoothstep);
        }
        else if (keyboardEvent.Key == SKey.W)
        {
            _barChart?.Hide(0.75d, Easings.Land);
        }
    }

    private ChartText AlternateDataFormat(double dietaryCalories)
    {
        var joules = dietaryCalories * 4184d;
        var wattage = joules / 86400d;

        var jouleText = TickFormatters.NaNAsDash(j => TickFormatters.QuantityToSIPrefixed(j, "J"))(joules);
        var wattText = TickFormatters.NaNAsDash(w => TickFormatters.QuantityToSIPrefixed(w, "W"))(wattage);

        return ChartText.Concat(ChartText.Concat(jouleText, ChartText.String(" / ")), wattText);
    }
}
