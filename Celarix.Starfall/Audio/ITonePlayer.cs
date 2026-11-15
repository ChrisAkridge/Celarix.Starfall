namespace Celarix.Starfall.Audio;

/// <summary>
/// Plays short sine tones, like the beeps in sorting-algorithm videos. Implementations decide where
/// the sound goes (speakers, a recorded track, or nowhere); slides only ask for a tone.
/// </summary>
public interface ITonePlayer
{
    /// <summary>
    /// Schedules a tone and returns immediately. Overlapping tones mix together.
    /// </summary>
    /// <param name="delaySeconds">How long from now the tone starts, for spreading notes within a step.</param>
    /// <param name="decaySeconds">
    /// The time constant of an exponential fade-out, for notes that should ring and die away; null
    /// holds the note at full volume for its whole duration.
    /// </param>
    void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f,
        double delaySeconds = 0d, double? decaySeconds = null);
}

/// <summary>
/// A tone player that stays silent. This is the default on every platform until a host sets a real one.
/// </summary>
public sealed class NullTonePlayer : ITonePlayer
{
    public static NullTonePlayer Instance { get; } = new();

    private NullTonePlayer() { }

    public void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f,
        double delaySeconds = 0d, double? decaySeconds = null) { }
}
