using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class StarfallElement : AtriaElement
    {
        private struct CloudCircle
        {
            public SPointF Center { get; set; }
            public double Radius { get; set; }
            public SColor Color { get; set; }

            public readonly SRectF Bounds
            {
                get
                {
                    var diameter = Radius * 2;
                    var topLeft = Center.Left(Radius).Up(Radius);
                    return topLeft.WithSize(new SSizeF(diameter, diameter));
                }
            }

            public CloudCircle(SPointF center, double radius, SColor color)
            {
                Center = center;
                Radius = radius;
                Color = color;
            }
        }

        private struct FallingStar
        {
            public SPointF Center { get; set; }
            public int ShrinkFramesRemaining { get; set; }
            public double SizeMultiple { get; set; }
            public SAngle Rotation { get; set; }

            public FallingStar(SPointF center)
            {
                Center = center;
                SizeMultiple = 1d;
                Rotation = SAngle.FromDegrees(0d);
                ShrinkFramesRemaining = FallingStarShrinkDuration;
            }
        }

        private const int SpawnCloudWhenRightEdgeHasThisManyCloudsOrFewer = 8;
        private const double InitialCloudFillMaxRadiusDistance = 50d;
        private const double CloudDriftSpeed = 1d; // pixels per frame
        private const double CloudMinRadius = 100d;
        private const double CloudMaxRadius = 110d;
        private const double CloudMinY = -80d;
        private const double CloudMaxY = 200d;
        private const double FallingStarSpawnChancePerFrame = 3d / 60d;
        private const double FallingStarXVelocity = -8d; // pixels per frame
        private const double FallingStarYVelocity = 8d; // pixels per frame
        private const int FallingStarShrinkDuration = 90; // frames
        private const int FallingStarGlowExpansionDuration = 2; // frames
        private const double FallingStarGlowMultiplier = 2d;

        private static readonly SColor _circleColor = new SColor(58, 51, 153, 255);
        private static readonly SSizeF _circleSize = new SSizeF(50d, 50d);
        private static readonly SSizeF _starSize = new SSizeF(75d, 75d);
        private static readonly double _circleMargin = 25d;
        private static readonly SColor _cloudMinColor = new SColor(137, 153, 163, 255);
        private static readonly SColor _cloudMaxColor = new SColor(132, 154, 169, 255);
        private static readonly SAngle _fallingStarRotationalVelocity = SAngle.FromDegrees(-10d); // degrees per frame
        private static readonly SColor[] _fallingStarColorGradient;
        private static readonly double[] _fallingStarColorPositions;

        private readonly SImage _grassHillsImage;
        private readonly SImage _fallingStarImage;
        private readonly List<CloudCircle> _clouds = new();
        private readonly List<FallingStar> _fallingStars = new();
        private readonly Random _random = new();
        private readonly AnimationContext _animationContext = new();
        private double _cloudDrawYOffset = -400d;

        static StarfallElement()
        {
            _fallingStarColorGradient =
            [
                new(255, 255, 230, 255),
                new(255, 220,  80, 255),
                new(255, 160,   0, 255),
                SColor.Transparent
            ];
            _fallingStarColorPositions =
            [
                0.0d,
                0.08d,
                0.22d,
                1d
            ];
        }

        public StarfallElement(string atriaIdString)
        {
            Id = AtriaId.Parse(atriaIdString);
            _grassHillsImage = SImage.FromFile("Assets/Images/night_ground.png");
            _fallingStarImage = SImage.FromFile("Assets/Images/hp_gold_star.png");
        }

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);

            if (_clouds.Count == 0)
            {
                // Fill the sky with clouds!
                // CANIMPROVE: We really need some kind of Initialize method on elements we can call
                // where stuff like Slide, MeasurementService, and Bounds are available.
                var x = CloudMinY;  // NOT sic
                while (x < Bounds.Right + CloudMaxY)
                {
                    SpawnCloud(x);
                    x += _random.NextDouble() * InitialCloudFillMaxRadiusDistance;
                }
            }

            for (var i = 0; i < _clouds.Count; i++)
            {
                var cloud = _clouds[i];
                cloud.Center = cloud.Center.Left(CloudDriftSpeed);
                _clouds[i] = cloud;

                // If the cloud is fully to the left of the element, remove it.
                if (cloud.Bounds.Right < Bounds.Left)
                {
                    _clouds.RemoveAt(i);
                    i--;
                }
            }

            // Check to see if we can spawn a new cloud bubble. We can spawn a new cloud if there are
            // fewer than a constant number clouds currently intersecting the right edge of the element.
            var canSpawnCloud = _clouds.Select(c => MathHelpers.RectangleIntersectsXCoordinate(c.Bounds, Bounds.Right))
                .Count(i => i) <= SpawnCloudWhenRightEdgeHasThisManyCloudsOrFewer;
            if (canSpawnCloud)
            {
                SpawnCloud(Bounds.Right);
            }

            // Check to see if we can spawn a new falling star. We can spawn a new falling star if a random
            // number is less than the constant chance per frame.
            var canSpawnFallingStar = _random.NextDouble() < FallingStarSpawnChancePerFrame && _cloudDrawYOffset >= 0d;
            if (canSpawnFallingStar)
            {
                // Only spawn them in the right half of the sky, since they fall so fast to the left.
                var x = MathHelpers.RandomInRange(_random, Bounds.Center.X, Bounds.Right);
                var y = MathHelpers.RandomInRange(_random, 0d, CloudMaxY / 2d);
                _fallingStars.Add(new FallingStar(new SPointF(x, y)));
            }

            // Move the falling stars and remove any that have fallen below the bottom of the element.
            for (var i = 0; i < _fallingStars.Count; i++)
            {
                var star = _fallingStars[i];
                star.Center = star.Center.Move(FallingStarXVelocity, FallingStarYVelocity);
                star.Rotation += _fallingStarRotationalVelocity;

                var progress = 1d - ((double)star.ShrinkFramesRemaining / FallingStarShrinkDuration);
                var easedProgress = MathHelpers.Ease(1d, 0d, progress, Easings.Land);
                star.SizeMultiple = easedProgress;
                star.ShrinkFramesRemaining--;

                _fallingStars[i] = star;
                if (star.Center.Y > Bounds.Bottom)
                {
                    _fallingStars.RemoveAt(i);
                    i--;
                }
            }

            // Check to see if we need to move the clouds down.
            if (_cloudDrawYOffset == -400d)
            {
                _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
                {
                    _cloudDrawYOffset = MathHelpers.Ease(-400d, 0d, p, Easings.Land);
                }));
            }
        }

        private void SpawnCloud(double x)
        {
            var radius = MathHelpers.RandomInRange(_random, CloudMinRadius, CloudMaxRadius);
            var colorProgress = _random.NextDouble();
            var color = MathHelpers.InterpolateColor(_cloudMinColor, _cloudMaxColor, colorProgress);
            var yCoordinate = MathHelpers.RandomInRange(_random, CloudMinY, CloudMaxY);
            var xCoordinate = x + radius;
            _clouds.Add(new CloudCircle(new(xCoordinate, yCoordinate), radius, color));
        }

        public override void Render(IRenderTarget target)
        {
            // Draw the background circles first.
            var oddRowStartX = 0d;
            var evenRowStartX = -(_circleSize.Width / 2d);
            var rowCount = (int)Math.Ceiling(Bounds.Height / (_circleSize.Height + _circleMargin));
            for (var y = 0; y < rowCount; y++)
            {
                var x = y % 2 == 0 ? evenRowStartX : oddRowStartX;
                while (x < Bounds.Width)
                {
                    var circleBounds = new SRectF(x, y * (_circleSize.Height + _circleMargin), _circleSize.Width, _circleSize.Height);
                    target.DrawEllipse(circleBounds.Center, _circleSize, _circleColor.WithOpacity(Opacity), SPaintStyle.Fill);
                    x += _circleSize.Width + _circleMargin;
                }
            }

            // Now draw the falling stars.
            foreach (var star in _fallingStars)
            {
                var starSize = _starSize * star.SizeMultiple;
                var starBounds = starSize.CenterAt(star.Center);
                target.DrawImage(_fallingStarImage, starBounds, Opacity, star.Rotation);
            }

            // Now draw the clouds.
            foreach (var cloud in _clouds)
            {
                target.DrawEllipse(cloud.Center.Down(_cloudDrawYOffset), cloud.Bounds.Size, cloud.Color.WithOpacity(Opacity), SPaintStyle.Fill);
            }

            // Draw the glow above the clouds and the stars. This lets it look like the stars shine
            // through the clouds.
            foreach (var star in _fallingStars)
            {
                var starSize = _starSize * star.SizeMultiple;
                var starBounds = starSize.CenterAt(star.Center);
                var glowRadius = (starBounds.Width / 2d) * FallingStarGlowMultiplier;

                if (star.ShrinkFramesRemaining > FallingStarShrinkDuration - FallingStarGlowExpansionDuration)
                {
                    var expansionFrameNumber = FallingStarShrinkDuration - star.ShrinkFramesRemaining;

                    var progress = (double)expansionFrameNumber / FallingStarGlowExpansionDuration;
                    var easedProgress = MathHelpers.Ease(0d, 1d, progress, Easings.TakeOff);
                    glowRadius *= easedProgress;
                }

                target.DrawRadialGradientCircle(star.Center, glowRadius, _fallingStarColorGradient, _fallingStarColorPositions, SShaderTileMode.Clamp, SBlendMode.Plus);
            }

            // Now draw the grass hills.
            target.DrawImage(_grassHillsImage, Bounds, Opacity);
        }
    }
}
