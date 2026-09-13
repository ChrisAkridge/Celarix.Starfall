namespace Celarix.Starfall.Layout.Atria;

/// <summary>
/// A single frame of Atria execution. This is introduced now for future update-loop migration.
/// </summary>
public readonly record struct FrameTime(long Number, TimeSpan Elapsed, TimeSpan Delta);
