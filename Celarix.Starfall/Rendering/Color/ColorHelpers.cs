using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Color;

public static class ColorHelpers
{
    public static IEnumerable<SColor> LightnessRamp(SColor baseColor, double lightnessStepSize, int stepCount)
    {
        var baseOklch = new OklchColor(baseColor);
        var remainingLightness = 1.0 - baseOklch.Lightness;
        var requestedLightness = lightnessStepSize * stepCount;

        if (requestedLightness > remainingLightness)
        {
            lightnessStepSize = remainingLightness / stepCount;
        }

        for (var i = 0; i < stepCount; i++)
        {
            var newLightness = baseOklch.Lightness + (lightnessStepSize * (i + 1));
            yield return new OklchColor(newLightness, baseOklch.Chroma, baseOklch.Hue).ToSColor();
        }
    }
}
