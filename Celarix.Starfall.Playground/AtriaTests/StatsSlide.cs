using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests;

internal sealed class StatsSlide : AtriaSlide
{
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

        var barChart = new BarChartElement(data, "#barChart")
        {
            Size = new SSizeF(Size.Width * 0.8, Size.Height * 0.8),
        };
        var anchor = new BasisPoint(Center, "#barChartAnchor");
        barChart.AnchorCenterTo(anchor);
        Add([barChart, anchor])
            .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);

        var animation = FixedDurationAnimation.StartIn(60, 180,
            d =>
            {
                var max = 1000d;
                var min = 100d;
                var newValue = min + (max - min) * d;
                barChart.YGridlineSpacing = newValue;
            });
        Animations.ScheduleAnimation(animation);
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        Animations.Update(AtriaLayoutEngine.GlobalFrameNumber);
    }
}
