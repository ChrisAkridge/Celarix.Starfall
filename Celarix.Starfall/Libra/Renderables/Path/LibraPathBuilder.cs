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
    }
}
