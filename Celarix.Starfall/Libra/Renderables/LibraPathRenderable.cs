using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables
{
    public sealed class LibraPathRenderable : LibraRenderable
    {
        private readonly IReadOnlyList<LibraPathCommand> _pathCommands;
        private readonly PathStyle _pathStyle;

        public LibraPathRenderable(
            LibraRenderableKey key,
            IReadOnlyList<LibraPathCommand> pathCommands,
            PathStyle pathStyle)
            : base(key, SColor.Transparent, SColor.Transparent)
        {
            _pathCommands = pathCommands;
            _pathStyle = pathStyle;
        }

        public override LibraRenderable Clone()
        {
            var clonedPathCommands = new List<LibraPathCommand>(_pathCommands.Count);
            foreach (var command in _pathCommands)
            {
                LibraPathCommand clonedCommand = command switch
                {
                    ClosePath closePath => new ClosePath(),
                    CubicTo cubicTo => new CubicTo(cubicTo.Control1X, cubicTo.Control1Y, cubicTo.Control2X, cubicTo.Control2Y, cubicTo.X, cubicTo.Y),
                    LineTo lineTo => new LineTo(lineTo.X, lineTo.Y),
                    MoveTo moveTo => new MoveTo(moveTo.X, moveTo.Y),
                    QuadraticTo quadraticTo => new QuadraticTo(quadraticTo.ControlX, quadraticTo.ControlY, quadraticTo.X, quadraticTo.Y),
                    _ => throw new NotSupportedException($"Unsupported path command type: {command.GetType().Name}")
                };
                clonedPathCommands.Add(clonedCommand);
            }

            var clonedPathStyle = new PathStyle(_pathStyle.Fill, _pathStyle.Stroke, _pathStyle.StrokeWidth, _pathStyle.Cap, _pathStyle.Join);
            return new LibraPathRenderable(Key, clonedPathCommands, clonedPathStyle)
            {
                Position = Position,
            };
        }

        public override void RenderAt(IRenderTarget target, SPointF position, double scaleFactor)
        {
            var sPathCommands = LibraHelpers.ToSPathCommands(LibraHelpers.Translate(_pathCommands, position));
            var sPathStyle = _pathStyle.ToSPathStyle();
            target.DrawPath(sPathCommands, sPathStyle.WithOpacity(Opacity));
        }
    }
}
