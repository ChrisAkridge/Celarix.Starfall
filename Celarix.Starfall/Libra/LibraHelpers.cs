using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public static class LibraHelpers
    {
        public static IEnumerable<LibraPathCommand> Translate(this IEnumerable<LibraPathCommand> libraPathCommands,
            SPointF distance)
        {
            foreach (var command in libraPathCommands)
            {
                switch (command)
                {
                    case MoveTo moveTo:
                        yield return new MoveTo(moveTo.X + distance.X, moveTo.Y + distance.Y);
                        break;
                    case LineTo lineTo:
                        yield return new LineTo(lineTo.X + distance.X, lineTo.Y + distance.Y);
                        break;
                    case QuadraticTo quadraticTo:
                        yield return new QuadraticTo(
                            quadraticTo.ControlX + distance.X,
                            quadraticTo.ControlY + distance.Y,
                            quadraticTo.X + distance.X,
                            quadraticTo.Y + distance.Y
                        );
                        break;
                    case CubicTo cubicTo:
                        yield return new CubicTo(
                            cubicTo.Control1X + distance.X,
                            cubicTo.Control1Y + distance.Y,
                            cubicTo.Control2X + distance.X,
                            cubicTo.Control2Y + distance.Y,
                            cubicTo.X + distance.X,
                            cubicTo.Y + distance.Y
                        );
                        break;
                    case ClosePath: yield return new ClosePath(); break;
                    default:
                        throw new NotSupportedException($"Unsupported command type: {command.GetType().Name}");
                }
            }
        }

        public static IEnumerable<SPathCommand> ToSPathCommands(this IEnumerable<LibraPathCommand> libraPathCommands)
        {
            foreach (var command in libraPathCommands)
            {
                switch (command)
                {
                    case MoveTo moveTo:
                        yield return new SMoveTo(moveTo.X, moveTo.Y);
                        break;
                    case LineTo lineTo:
                        yield return new SLineTo(lineTo.X, lineTo.Y);
                        break;
                    case QuadraticTo quadraticTo:
                        yield return new SQuadraticTo(quadraticTo.ControlX, quadraticTo.ControlY, quadraticTo.X, quadraticTo.Y);
                        break;
                    case CubicTo cubicTo:
                        yield return new SCubicTo(cubicTo.Control1X, cubicTo.Control1Y, cubicTo.Control2X, cubicTo.Control2Y, cubicTo.X, cubicTo.Y);
                        break;
                    case ClosePath: yield return new SClosePath(); break;
                    default:
                        throw new NotSupportedException($"Unsupported command type: {command.GetType().Name}");
                }
            }
        }

        public static SPathStyle ToSPathStyle(this PathStyle libraPathStyle)
        {
            return new SPathStyle(
                libraPathStyle.Fill,
                libraPathStyle.Stroke,
                libraPathStyle.StrokeWidth,
                libraPathStyle.Cap,
                libraPathStyle.Join
            );
        }
    }
}
