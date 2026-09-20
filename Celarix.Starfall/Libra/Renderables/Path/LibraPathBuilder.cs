using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables.Path
{
    public sealed class LibraPathBuilder
    {
        private readonly List<LibraPathCommand> _commands = new List<LibraPathCommand>();

        public LibraPathBuilder()
        {
            _commands.Add(new MoveTo(0d, 0d));
        }

        public IReadOnlyList<LibraPathCommand> ClosePath()
        {
            _commands.Add(new ClosePath());
            return _commands;
        }

        public LibraPathBuilder CubicTo(double control1X, double control1Y, double control2X, double control2Y, double x, double y)
        {
            _commands.Add(new CubicTo(control1X, control1Y, control2X, control2Y, x, y));
            return this;
        }

        public LibraPathBuilder LineTo(double x, double y)
        {
            _commands.Add(new LineTo(x, y));
            return this;
        }

        public LibraPathBuilder MoveTo(double x, double y)
        {
            _commands.Add(new MoveTo(x, y));
            return this;
        }

        public LibraPathBuilder QuadraticTo(double controlX, double controlY, double x, double y)
        {
            _commands.Add(new QuadraticTo(controlX, controlY, x, y));
            return this;
        }

        public static IReadOnlyList<LibraPathCommand> MirrorHorizontally(IReadOnlyList<LibraPathCommand> commands, double width)
        {
            var mirroredCommands = new List<LibraPathCommand>(commands.Count);
            foreach (var command in commands)
            {
                switch (command)
                {
                    case MoveTo moveTo:
                        mirroredCommands.Add(new MoveTo(width - moveTo.X, moveTo.Y));
                        break;
                    case LineTo lineTo:
                        mirroredCommands.Add(new LineTo(width - lineTo.X, lineTo.Y));
                        break;
                    case QuadraticTo quadraticTo:
                        mirroredCommands.Add(new QuadraticTo(width - quadraticTo.ControlX, quadraticTo.ControlY, width - quadraticTo.X, quadraticTo.Y));
                        break;
                    case CubicTo cubicTo:
                        mirroredCommands.Add(new CubicTo(width - cubicTo.Control1X, cubicTo.Control1Y, width - cubicTo.Control2X, cubicTo.Control2Y, width - cubicTo.X, cubicTo.Y));
                        break;
                    case ClosePath closePath:
                        mirroredCommands.Add(closePath);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown command type: {command.GetType().Name}");
                }
            }
            return mirroredCommands;
        }
    }
}
