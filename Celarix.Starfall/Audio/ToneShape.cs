namespace Celarix.Starfall.Audio;

/// <summary>
/// The volume envelope of a tone: how it fades in, whether it decays, and how it fades out.
/// </summary>
/// <param name="FadeInSeconds">The fade-in at the start. Short fades give an audible attack.</param>
/// <param name="FadeOutSeconds">The fade-out at the end.</param>
/// <param name="DecaySeconds">
/// If set, the time constant of an exponential decay after the fade-in, so the note rings and dies away.
/// </param>
/// <param name="Smooth">
/// Whether fades follow a raised-cosine curve instead of a straight line. Two overlapping
/// raised-cosine fades always add up to full volume, so back-to-back notes can crossfade without a
/// dip or a pop.
/// </param>
public readonly record struct ToneShape(double FadeInSeconds, double FadeOutSeconds, double? DecaySeconds = null, bool Smooth = false)
{
    /// <summary>Five-millisecond straight fades and no decay: a plain beep with a slight attack.</summary>
    public static ToneShape Default { get; } = new(0.005d, 0.005d);

    /// <summary>A short attack, then an exponential decay with the given time constant.</summary>
    public static ToneShape Ringing(double decaySeconds) => new(0.005d, 0.005d, decaySeconds);

    /// <summary>
    /// Smooth fades of <paramref name="seconds"/> at both ends. Give each note a duration of twice the
    /// spacing and fades equal to the spacing, and consecutive notes crossfade into one another.
    /// </summary>
    public static ToneShape Crossfade(double seconds) => new(seconds, seconds, null, Smooth: true);
}
