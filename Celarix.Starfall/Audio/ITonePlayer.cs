namespace Celarix.Starfall.Audio;

/// <summary>
/// Plays short tones, like the beeps in sorting-algorithm videos. Implementations decide where the
/// sound goes (speakers, a recorded track, or nowhere); slides only ask for a tone.
/// </summary>
public interface ITonePlayer
{
    /// <summary>
    /// Starts a tone and returns immediately. Overlapping tones mix together.
    /// </summary>
    void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f);
}

/// <summary>
/// A tone player that stays silent. This is the default on every platform until a host sets a real one.
/// </summary>
public sealed class NullTonePlayer : ITonePlayer
{
    public static NullTonePlayer Instance { get; } = new();

    private NullTonePlayer() { }

    public void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f) { }
}
