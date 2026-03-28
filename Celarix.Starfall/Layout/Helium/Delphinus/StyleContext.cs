using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus
{
    internal sealed record StyleContext(
        string FontFamily,
        string FontSize,
        string BgColor,
        string Color,
        string? Id,
        string? Classes,
        string HAlign,
        string VAlign,
        string LineMargin
    )
    {
        public static StyleContext StartingContext => new StyleContext(
            FontFamily: "Arial",
            FontSize: "12",
            BgColor: "#FFFFFF00",
            Color: "#000000FF",
            Id: null,
            Classes: null,
            HAlign: "left",
            VAlign: "top",
            LineMargin: "0"
        );
    };
}
