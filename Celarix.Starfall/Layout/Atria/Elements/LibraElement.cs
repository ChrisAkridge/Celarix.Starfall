using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Metrics;
using Celarix.Starfall.Libra.Metrics.Symbols;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements
{
    public sealed class LibraElement : AtriaElement
    {
        private enum ExpressionLayoutState
        {
            // The expression has been laid out, all renderables are up to date, and no animation
            // is currently running.
            Clean,
            // The renderables are not up to date. Lay them out on the next frame and immediately
            // begin rendering them.
            Dirty,
            // We are animating a temporary set of renderables representing a transformation from one
            // expression to another. Once the animation is done, we will lay out the new expression and render it.
            AnimatingTransform
        }

        private IReadOnlyList<LibraRenderable> _renderables;
        private ExpressionLayoutState _state;
        private LibraMetrics _metrics;
        private LibraExpression _root;
        private LibraExpression? _rootAfterTransform;
        private SSizeF _layoutSize;

        public SFont BaseFont { get; set; }
        public double ScaleFactor { get; set; }

        public LibraElement(LibraExpression root, SFont baseFont, string atriaIdString)
        {
            _root = root;
            BaseFont = baseFont;
            ScaleFactor = 1d;
            _renderables = Array.Empty<LibraRenderable>();
            _state = ExpressionLayoutState.Dirty;
            _metrics = new LibraMetrics
            {
                BinaryExpressions = new BinaryExpressionMetrics(),
                Fences = new FenceMetrics
                {
                    ParenthesesMetrics = new ParenthesesMetrics(),
                },
                Fractions = new FractionMetrics(),
                Scripts = new ScriptsMetrics()
            };

            if (BaseFont.Size == null)
            {
                BaseFont = BaseFont.WithSize(12f);
            }

            Id = AtriaId.Parse(atriaIdString);
        }

        public override void Update(double deltaTime)
        {
            if (_state == ExpressionLayoutState.Dirty)
            {
                UpdateRenderables(Slide?.MeasurementService ?? throw new InvalidOperationException("Slide must be set before updating renderables."));
            }

            Animations.Update(AtriaLayoutEngine.GlobalFrameNumber);
            base.Update(deltaTime);
        }

        public override void Render(IRenderTarget target)
        {
            // Apply the scaling factor.
            var scaledTotalBounds = new SRectF(SPointF.Zero, _layoutSize * ScaleFactor);

            // Figure out where to move the renderables based on the current position of this element.
            var anchoredPosition = scaledTotalBounds.GetEdgePoint(AnchoredPosition ?? Alignment.TopLeft);
            var offset = Position - anchoredPosition;

            foreach (var renderable in _renderables)
            {
                var scaledPosition = (renderable.Position * ScaleFactor) + offset;
                renderable.RenderAt(target, scaledPosition, ScaleFactor);
            }
        }

        private void UpdateRenderables(MeasurementService measurementService)
        {
            if (_state != ExpressionLayoutState.Dirty)
            {
                return;
            }

            var context = new LibraRenderingContext(measurementService, BaseFont, _metrics, FenceRenderingMode.Procedural);
            var layout = _root.Layout(context);
            _renderables = layout.Renderables;
            _layoutSize = layout.Bounds.Size;
            LibraExpression.IdsAreUniqueOrThrow(_root);
            _state = ExpressionLayoutState.Clean;
        }


        // Transforms and Animations
        public void TransformAnimate(Func<LibraExpression, LibraExpression> transform, double duration)
        {
            if (_state == ExpressionLayoutState.AnimatingTransform)
            {
                Console.WriteLine("Cannot start a new transform animation while another is in progress.");
                return;
            }

            _state = ExpressionLayoutState.AnimatingTransform;
            _rootAfterTransform = transform(_root);

            var durationInFrames = AnimationContext.SecondsToFrames(duration);
            var renderingContext = new LibraRenderingContext(Slide?.MeasurementService
                ?? throw new InvalidOperationException("Slide must be set before transforming."), BaseFont,
                _metrics);

            LibraLayoutResult oldLayout = _root.Layout(renderingContext);
            LibraLayoutResult newLayout = _rootAfterTransform.Layout(renderingContext);
            var oldLayoutSize = oldLayout.Bounds.Size;
            var newLayoutSize = newLayout.Bounds.Size;

            var oldRenderables = oldLayout
                .Renderables
                .ToDictionary(r => r.Key, r => r);
            var newRenderables = newLayout
                .Renderables
                .ToDictionary(r => r.Key, r => r);

            // Determine which renderables are entering (new), exiting (old), and persisting (new and old).
            var enteringRenderables = newRenderables
                .Where(kvp => !oldRenderables.ContainsKey(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();
            var exitingRenderables = oldRenderables
                .Where(kvp => !newRenderables.ContainsKey(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();
            var persistingRenderables = newRenderables
                .Where(kvp => oldRenderables.ContainsKey(kvp.Key))
                .Select(kvp => (Old: oldRenderables[kvp.Key], New: kvp.Value))
                .ToList();

            // Change the renderables to the intermediate set immediately.
            foreach (var enteringRenderable in enteringRenderables)
            {
                InitializeEnteringRenderable(enteringRenderable);
            }
            _renderables = [.. enteringRenderables, .. exitingRenderables, .. persistingRenderables.Select(pair => pair.Old)];

            var enteringAnimations = enteringRenderables
                .Select(BuildEnteringAnimation)
                .ToList();
            var exitingAnimations = exitingRenderables
                .Select(BuildExitingAnimation)
                .ToList();
            var persistingAnimations = persistingRenderables
                .Select(pair => BuildPersistingAnimation(pair.Old, pair.New))
                .ToList();

            // Group all animations into a single animation and schedule it.
            _layoutSize = oldLayoutSize; // Start with the old layout size for the animation.
            var allAnimations = enteringAnimations
                .Concat(exitingAnimations)
                .Concat(persistingAnimations)
                .Append(BuildLayoutSizeAnimation(oldLayoutSize, newLayoutSize))
                .ToArray();
            var fixedDurationAnimation = new FixedDurationAnimation(
                AtriaLayoutEngine.GlobalFrameNumber,
                durationInFrames,
                BuildCompositeAnimation(allAnimations),
                () => OnCompleted(newLayoutSize));
            Animations.ScheduleAnimation(fixedDurationAnimation);
        }

        private void InitializeEnteringRenderable(LibraRenderable renderable)
        {
            // For entering renderables, we can start them with opacity 0.
            renderable.Opacity = 0;
        }

        private Action<double> BuildEnteringAnimation(LibraRenderable renderable)
        {
            return (progress) =>
            {
                // For entering renderables, we can animate their opacity from 0 to 1.
                renderable.Opacity = progress;
            };
        }

        private Action<double> BuildExitingAnimation(LibraRenderable renderable)
        {
            var startOpacity = renderable.Opacity;

            return (progress) =>
            {
                // For exiting renderables, we can animate their opacity from 1 to 0.
                renderable.Opacity = startOpacity * (1 - progress);
            };
        }

        private Action<double> BuildPersistingAnimation(LibraRenderable oldRenderable, LibraRenderable newRenderable)
        {
            var startPosition = oldRenderable.Position;
            var endPosition = newRenderable.Position;

            var startSize = oldRenderable.Size;
            var endSize = newRenderable.Size;

            var startForegroundColor = oldRenderable.ForegroundColor;
            var endForegroundColor = newRenderable.ForegroundColor;

            var startBackgroundColor = oldRenderable.BackgroundColor;
            var endBackgroundColor = newRenderable.BackgroundColor;

            return (progress) =>
            {
                oldRenderable.Position = MathHelpers.Ease(
                    startPosition,
                    endPosition,
                    progress,
                    Easings.Land);

                oldRenderable.Size = MathHelpers.Ease(
                    startSize,
                    endSize,
                    progress,
                    Easings.Land);

                oldRenderable.ForegroundColor = MathHelpers.InterpolateColor(
                    startForegroundColor,
                    endForegroundColor,
                    progress);

                oldRenderable.BackgroundColor = MathHelpers.InterpolateColor(
                    startBackgroundColor,
                    endBackgroundColor,
                    progress);
            };
        }

        private Action<double> BuildLayoutSizeAnimation(SSizeF oldSize, SSizeF newSize)
        {
            return (progress) =>
            {
                _layoutSize = MathHelpers.Ease(
                    oldSize,
                    newSize,
                    progress,
                    Easings.Land);
            };
        }

        private Action<double> BuildCompositeAnimation(IReadOnlyList<Action<double>> animations)
        {
            return (progress) =>
            {
                foreach (var animation in animations)
                {
                    animation(progress);
                }

                var scaledTotalBounds = new SRectF(SPointF.Zero, _layoutSize * ScaleFactor);
                var anchoredPosition = scaledTotalBounds.GetEdgePoint(AnchoredPosition ?? Alignment.TopLeft);
                var offset = Position - anchoredPosition;
                Console.WriteLine($"Progress: {progress}, "
                    + $"Layout Size: {_layoutSize}, "
                    + $"Anchor Point: {anchoredPosition}, "
                    + $"Offset: {offset}");
            };
        }

        private void OnCompleted(SSizeF newLayoutSize)
        {
            // Once the animation is complete, we finalize the state and update the renderables to the new expression.
            _root = _rootAfterTransform ?? throw new InvalidOperationException("Root after transform should not be null when completing animation.");
            _rootAfterTransform = null;
            _layoutSize = newLayoutSize;
            _state = ExpressionLayoutState.Dirty;
        }
    }
}
