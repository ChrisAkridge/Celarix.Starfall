using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall
{
    public enum ErrorLevel
    {
        /// <summary>
        /// Errors are silently ignored. This is not recommended, but comes in handy in a live demo scenario.
        /// </summary>
        Silent,

        /// <summary>
        /// Errors are rendered in the scene but do not stop the engine.
        /// This is the default behavior and is recommended.
        /// </summary>
        Display,

        /// <summary>
        /// Errors are thrown as exceptions and stop the engine.
        /// This is useful for debugging but not recommended for production use.
        /// </summary>
        Exception
    }

    public enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        LeftCenter,
        Center,
        RightCenter,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum Unit
    {
        Fraction,
        Pixel
    }

    [Flags]
    public enum Sides
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        All = Left | Top | Right | Bottom
    }
}
