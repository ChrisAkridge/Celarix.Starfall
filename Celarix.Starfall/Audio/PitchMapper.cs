namespace Celarix.Starfall.Audio;

/// <summary>
/// How a pitch mapper spreads values between its lowest and highest frequency.
/// </summary>
public enum PitchScale
{
    /// <summary>Equal steps in hertz, as in most sorting videos. High values crowd together to the ear.</summary>
    Linear,

    /// <summary>Equal musical intervals, so every step up sounds like the same amount higher.</summary>
    Exponential
}

/// <summary>
/// Maps an index (or value) in a range to a frequency, so a bigger value sounds higher.
/// </summary>
public sealed class PitchMapper
{
    /// <summary>Gets or sets the frequency of the lowest value. Defaults to 120 Hz.</summary>
    public double MinimumHz { get; init; } = 120d;

    /// <summary>Gets or sets the frequency of the highest value. Defaults to 1200 Hz.</summary>
    public double MaximumHz { get; init; } = 1200d;

    /// <summary>Gets or sets how values are spread between the two. Defaults to exponential.</summary>
    public PitchScale Scale { get; init; } = PitchScale.Exponential;

    /// <summary>
    /// Gets the frequency for <paramref name="index"/> out of <paramref name="count"/> values (0 to count − 1).
    /// Indices outside the range are clamped.
    /// </summary>
    public double FrequencyFor(double index, double count)
    {
        if (count <= 1) { return MinimumHz; }
        var fraction = Math.Clamp(index / (count - 1), 0d, 1d);
        return Scale switch
        {
            PitchScale.Linear => MinimumHz + (MaximumHz - MinimumHz) * fraction,
            _ => MinimumHz * Math.Pow(MaximumHz / MinimumHz, fraction)
        };
    }
}
