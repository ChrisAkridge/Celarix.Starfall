namespace Celarix.Starfall.Layout.Atria;

/// <summary>
/// A single frame of Atria execution.
/// </summary>
public readonly record struct FrameTime(int Number, TimeSpan Elapsed, TimeSpan Delta);
