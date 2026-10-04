using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Atria;

/// <summary>
/// Describes the positional and opacity changes produced by a visual transition.
/// </summary>
public readonly record struct TransitionState(SPointF Offset, double Opacity);
