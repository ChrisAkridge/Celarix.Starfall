using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Libra;
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
        private AnimationContext _animationContext;
        private IReadOnlyList<LibraRenderable> _renderables;
        private bool _dirty;
        private SRectF _totalBounds;
        private LibraExpression _root;

        public SFont BaseFont { get; set; }
        public double ScaleFactor { get; set; }

        public LibraElement(LibraExpression root, SFont baseFont)
        {
            _root = root;
            BaseFont = baseFont;
            ScaleFactor = 1d;
            _animationContext = new AnimationContext();
            _renderables = Array.Empty<LibraRenderable>();
            _dirty = true;

            if (BaseFont.Size == null)
            {
                BaseFont = BaseFont.WithSize(12f);
            }
        }

        public override void Update(double deltaTime)
        {
            if (_dirty)
            {
                UpdateRenderables(Slide?.MeasurementService ?? throw new InvalidOperationException("Slide must be set before updating renderables."));
            }

            base.Update(deltaTime);
        }

        public override void Render(IRenderTarget target)
        {
            // Apply the scaling factor.
            var scaledTotalBounds = new SRectF(_totalBounds.Position, _totalBounds.Size * ScaleFactor);

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
            if (!_dirty)
            {
                return;
            }

            var context = new LibraRenderingContext(measurementService, BaseFont);
            _renderables = _root.Layout(context).Renderables;
            _totalBounds = SRectF.BoundsOf(_renderables.Select(r => r.Bounds));
            _dirty = false;
        }
    }
}
