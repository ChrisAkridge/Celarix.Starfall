using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Mathematics;
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

        public LibraPathRenderable MirrorHorizontally(double width)
        {
            var mirroredCommands = LibraPathBuilder.MirrorHorizontally(_pathCommands, width);
            return new LibraPathRenderable(Key, mirroredCommands, _pathStyle)
            {
                Position = Position,
            };
        }

        public SRectF GetTrueBounds()
        {
            var bounds = new PathBoundsBuilder();
            var current = SPointF.Zero;
            var subpathStart = SPointF.Zero;

            foreach (var command in _pathCommands)
            {
                switch (command)
                {
                    case MoveTo move:
                        current = new SPointF(move.X, move.Y);
                        subpathStart = current;
                        bounds.Include(current);
                        break;

                    case LineTo line:
                        {
                            var end = new SPointF(line.X, line.Y);
                            bounds.Include(current);
                            bounds.Include(end);
                            current = end;
                            break;
                        }

                    case QuadraticTo quadratic:
                        {
                            var control = new SPointF(
                                quadratic.ControlX,
                                quadratic.ControlY);

                            var end = new SPointF(
                                quadratic.X,
                                quadratic.Y);

                            bounds.IncludeQuadratic(current, control, end);
                            current = end;
                            break;
                        }

                    case CubicTo cubic:
                        {
                            var control1 = new SPointF(
                                cubic.Control1X,
                                cubic.Control1Y);

                            var control2 = new SPointF(
                                cubic.Control2X,
                                cubic.Control2Y);

                            var end = new SPointF(
                                cubic.X,
                                cubic.Y);

                            bounds.IncludeCubic(
                                current,
                                control1,
                                control2,
                                end);

                            current = end;
                            break;
                        }

                    case ClosePath:
                        bounds.Include(current);
                        bounds.Include(subpathStart);
                        current = subpathStart;
                        break;
                }
            }

            return bounds.Bounds;
        }
    }
}
