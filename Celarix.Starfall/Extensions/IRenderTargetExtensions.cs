using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions;

public static class IRenderTargetExtensions
{
    public static void DrawRectangleOfThickness(this IRenderTarget renderTarget, SRectF rect, double thickness, SColor color)
    {
        // Top
        renderTarget.DrawRectangle(new SRectF(rect.X, rect.Y, rect.Width, thickness), color, SPaintStyle.Fill, SAngle.Zero);
        // Bottom
        renderTarget.DrawRectangle(new SRectF(rect.X, rect.Bottom - thickness, rect.Width, thickness), color, SPaintStyle.Fill, SAngle.Zero);
        // Left
        renderTarget.DrawRectangle(new SRectF(rect.X, rect.Y + thickness, thickness, rect.Height - (thickness * 2)), color, SPaintStyle.Fill, SAngle.Zero   );
        // Right
        renderTarget.DrawRectangle(new SRectF(rect.Right - thickness, rect.Y + thickness, thickness, rect.Height - (thickness * 2)), color, SPaintStyle.Fill, SAngle.Zero);
    }
}
