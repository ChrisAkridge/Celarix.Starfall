using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Atria;

/// <summary>
/// Stateless transitions that can be applied to elements or individual visual objects.
/// </summary>
public static class Transitions
{
    /// <summary>
    /// Moves an object from <paramref name="offset"/> to its final position while fading it in.
    /// </summary>
    /// <param name="progress">Transition progress from 0 to 1.</param>
    /// <param name="offset">The object's displacement from its final position at progress 0.</param>
    /// <param name="easing">The easing applied to both motion and opacity.</param>
    public static TransitionState SlideFadeIn(double progress, SPointF offset, Easing easing)
    {
        ArgumentNullException.ThrowIfNull(easing);

        var easedProgress = easing(Math.Clamp(progress, 0d, 1d));
        return new TransitionState(
            offset * (1d - easedProgress),
            Math.Clamp(easedProgress, 0d, 1d));
    }
}
