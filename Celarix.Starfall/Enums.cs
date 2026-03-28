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

    public enum Direction
    {
        Horizontal,
        Vertical
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

    public enum HAlignment
    {
        Left,
        Center,
        Right
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

    // okay yes this is straight from SkiaSharp
    // but it's the first render target, so it will kind of shape things
    public enum FontWeight
    {
        Invisible = 0,
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
        ExtraBlack = 1000
    }

    public enum FontWidth
    {
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9
    }

    public enum FontSlant
    {
        Upright = 0,
        Italic = 1,
        Oblique = 2
    }

    public enum FadeDirection
    {
        In,
        Out
    }

    public enum LargeOperatorKind
    {
        Sum,
        Product,
        Integral,
        Limit
    }
}
