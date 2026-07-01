using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideSF_04_NoAbsolutePositioning : AtriaSlide
    {
        internal static readonly string[] imagePaths =
        [
            "Assets/Images/powerPointPositioning.png",
            "Assets/Images/wordPositioning.png",
            "Assets/Images/cssPositioning.png"
        ];

        private readonly Random _random = new();
        private int _state;
        private AnimationContext _animationContext = new();

        public SlideSF_04_NoAbsolutePositioning(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.StarfallBackground;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override SlideAdvanceResult Advance()
        {
            var points = MathHelpers.EquallySpaceCenteredPoints(LeftCenter.X, RightCenter.X, 3)
                    .Select(p => new SPointF(p, Center.Y))
                    .ToArray();

            if (_state == 0)
            {
                for (var i = 0; i < points.Length; i++)
                {
                    var point = points[i];
                    // Nudge the point a little so they look like I put them there by hand, badly
                    var xOffset = _random.NextDouble() * 50 - 25;
                    var yOffset = _random.NextDouble() * 50 - 25;
                    points[i] = new SPointF(point.X + xOffset, point.Y + yOffset);
                }

                var imageElements = imagePaths.Select((path, index) => ImageElement.FromFile(path, $"#image{index}"))
                    .ToArray();
                
                var imageAnchors = imageElements.Select((e, i) => new BasisPoint(points[i], $"#imageAnchor{i}")).ToArray();
                for (var i = 0; i < imageElements.Length; i++)
                {
                    var imageElement = imageElements[i];
                    var imageAnchor = imageAnchors[i];
                    imageElement.AnchorCenterTo(imageAnchor);
                }

                Add(imageElements.Cast<ISlideAddable>().Concat(imageAnchors.Cast<ISlideAddable>()))
                    .AnimateBasic(0.5, AnimationTypes.FadeIn, Easings.Linear);
                _state = 1;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (_state == 1)
            {
                var anchors = new[]
                {
                    (BasisPoint)QueryBasis("#imageAnchor0").Single(),
                    (BasisPoint)QueryBasis("#imageAnchor1").Single(),
                    (BasisPoint)QueryBasis("#imageAnchor2").Single()
                };

                var transformFuncs = anchors.Select((a, i) =>
                {
                    var fromPoint = a.Point;
                    var toPoint = points[i];
                    var delay = AnimationContext.SecondsToFrames(i * (1d / 3d));
                    var duration = AnimationContext.SecondsToFrames(0.5d);
                    Action<double> action = p =>
                    {
                        a.Point = MathHelpers.Ease(fromPoint, toPoint, p, Easings.Land);
                    };

                    return FixedDurationAnimation.StartIn(delay, duration, action);
                });

                foreach (var transformFunc in transformFuncs)
                {
                    _animationContext.ScheduleAnimation(transformFunc);
                }
                _state = 2;
                return SlideAdvanceResult.InternalStateChanged;
            }
            return SlideAdvanceResult.CanAdvance;
        }
    }
}
