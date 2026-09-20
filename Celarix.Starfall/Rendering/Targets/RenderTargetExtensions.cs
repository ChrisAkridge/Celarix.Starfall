using Celarix.Starfall.Rendering.Models;
using System;

namespace Celarix.Starfall.Rendering.Targets
{
    public static class RenderTargetExtensions
    {
        public static IDisposable Transformed(this IRenderTarget target, STransform2D transform)
        {
            target.PushTransform(transform);
            return new TransformScope(target);
        }

        private sealed class TransformScope : IDisposable
        {
            private readonly IRenderTarget _target;
            private bool _disposed;

            public TransformScope(IRenderTarget target)
            {
                _target = target;
            }

            public void Dispose()
            {
                if (_disposed) { return; }

                _target.PopTransform();
                _disposed = true;
            }
        }
    }
}
