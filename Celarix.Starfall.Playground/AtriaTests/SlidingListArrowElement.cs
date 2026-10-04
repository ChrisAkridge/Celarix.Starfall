using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Playground.AtriaTests;

/// <summary>
/// A white triangular pointer that enters along the bottom of the slide, then
/// rises into place while turning to point at content to its right.
/// </summary>
internal sealed class SlidingListArrowElement : AtriaElement
{
    private const double HorizontalDurationSeconds = 0.6d;
    private const double SettlingDelaySeconds = 0.01d;
    private const double RiseDurationSeconds = 0.55d;

    public const double ArrivalDurationSeconds =
        HorizontalDurationSeconds + SettlingDelaySeconds + RiseDurationSeconds;

    private static readonly Easing HorizontalEasing = t => Easings.LandFaster(t, 2.5d);

    private static readonly SPathStyle ArrowStyle = new(
        SColor.White,
        null,
        0d,
        SStrokeCap.Butt,
        SStrokeJoin.Miter);

    private double rotationDegrees;
    private bool animationStarted;

    /// <summary>
    /// Gets the X coordinate at which the arrow's right edge stops.
    /// </summary>
    public double TargetX { get; }

    public SlidingListArrowElement(double targetX, double width)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);

        TargetX = targetX;
        Size = new SSizeF(width, width);
        Opacity = 1d;
    }

    protected override void OnAttached()
    {
        var slideSize = Slide!.Size;
        Position = new SPointF(slideSize.Width, slideSize.Height - Size.Height);
    }

    public void StartAnimation()
    {
        if (animationStarted) { return; }
        animationStarted = true;

        var slideSize = Slide!.Size;
        var bottomPosition = new SPointF(slideSize.Width, slideSize.Height - Size.Height);
        var landedPosition = new SPointF(TargetX - Size.Width, bottomPosition.Y);
        var finalPosition = new SPointF(landedPosition.X, (slideSize.Height - Size.Height) / 2d);

        Position = bottomPosition;
        rotationDegrees = 0d;

        var horizontalFrames = AnimationContext.SecondsToFrames(HorizontalDurationSeconds);
        var settlingDelayFrames = AnimationContext.SecondsToFrames(SettlingDelaySeconds);
        var riseFrames = AnimationContext.SecondsToFrames(RiseDurationSeconds);
        var riseStartFrame = horizontalFrames + settlingDelayFrames;

        Animations.ScheduleAnimation(Animations.StartNow(horizontalFrames, progress =>
        {
            Position = Interpolators.Get<SPointF>().Interpolate(
                bottomPosition,
                landedPosition,
                HorizontalEasing(progress));
        }));

        ScheduleRise(landedPosition, finalPosition, riseStartFrame, riseFrames);
        ScheduleRotation(riseStartFrame, (int)(riseFrames * 1.25d));
    }

    private void ScheduleRise(SPointF from, SPointF to, int delayFrames, int durationFrames)
    {
        Animations.ScheduleAnimation(Animations.StartIn(delayFrames, durationFrames, progress =>
        {
            var easedProgress = Easings.Smoothstep(progress);
            Position = Interpolators.Get<SPointF>().Interpolate(from, to, easedProgress);
        }));
    }

    private void ScheduleRotation(int delayFrames, int durationFrames)
    {
        Animations.ScheduleAnimation(Animations.StartIn(delayFrames, durationFrames, progress =>
        {
            var easedProgress = Easings.BackOut(Easings.Smoothstep(progress), 1d);
            rotationDegrees = -90d * easedProgress;
        }));
    }

    public override void Render(IRenderTarget target)
    {
        var center = Bounds.Center;
        var points = new[]
        {
            RotateAroundCenter(new SPointF(Bounds.Left, Bounds.Top), center),
            RotateAroundCenter(new SPointF(Bounds.Right, Bounds.Top), center),
            RotateAroundCenter(new SPointF(center.X, Bounds.Bottom), center)
        };

        target.DrawPath(
        [
            new SMoveTo(points[0].X, points[0].Y),
            new SLineTo(points[1].X, points[1].Y),
            new SLineTo(points[2].X, points[2].Y),
            new SClosePath()
        ], ArrowStyle.WithOpacity(Opacity));
    }

    private SPointF RotateAroundCenter(SPointF point, SPointF center)
    {
        var radians = double.DegreesToRadians(rotationDegrees);
        var cosine = Math.Cos(radians);
        var sine = Math.Sin(radians);
        var x = point.X - center.X;
        var y = point.Y - center.Y;

        return new SPointF(
            center.X + (x * cosine) - (y * sine),
            center.Y + (x * sine) + (y * cosine));
    }
}
