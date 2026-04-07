using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    /// <summary>
    /// Represents a renderable that is cached as an image. This is useful for renderables that are
    /// expensive to render but don't change often, such as complex shapes or text. The cached image
    /// will be redrawn if the <see cref="Bounds"/> property changes. If you want to change the cached
    /// image's construction, create a new <see cref="CachedRenderable"/> instance with a different render function.
    /// </summary>
    public sealed class CachedRenderable : HeliumRenderable,
        IBoundedRenderable
    {
        private SImage? _cachedImage;
        private SSizeF? _cachedImageSize;
        private Func<SSizeF, IReadOnlyList<IRenderable>> _getRenderablesFunc;

        public CachedRenderable(Func<SSizeF, IReadOnlyList<IRenderable>> getRenderablesFunc, SRectF bounds)
        {
            _getRenderablesFunc = getRenderablesFunc;
            Bounds = bounds;
        }

        public SRectF Bounds { get; set; }

        public override void Render(IRenderTarget target)
        {
            if (_cachedImage == null
                || !_cachedImageSize.HasValue
                || _cachedImageSize.Value != Bounds.Size)
            {
                var offscreenTarget = target.CreateOffscreenTarget(Bounds.Size);
                var renderables = _getRenderablesFunc(Bounds.Size);
                foreach (var renderable in renderables)
                {
                    renderable.Render(offscreenTarget);
                }
                offscreenTarget.Complete();
                _cachedImage = offscreenTarget.CompletedImage;
                _cachedImageSize = Bounds.Size;
            }

            target.DrawImage(_cachedImage!, Bounds);
        }
    }
}
