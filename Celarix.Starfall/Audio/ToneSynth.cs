namespace Celarix.Starfall.Audio;

/// <summary>
/// Renders sine-wave tones into sample buffers. Platform-independent, so it can feed live
/// playback or an offline audio track alike.
/// </summary>
public static class ToneSynth
{
    public const int DefaultSampleRate = 44100;

    /// <summary>
    /// Renders a mono sine tone with a short fade in and out, so tones start and stop without clicks.
    /// </summary>
    /// <param name="fadeSeconds">The length of each fade. Clamped to half the tone.</param>
    /// <param name="decaySeconds">
    /// If set, the time constant of an exponential decay after the fade-in, so the note rings and dies away.
    /// </param>
    /// <param name="delaySeconds">Silence to put before the tone.</param>
    public static float[] RenderSine(double frequencyHz, double durationSeconds, float volume = 0.3f,
        int sampleRate = DefaultSampleRate, double fadeSeconds = 0.005d, double? decaySeconds = null,
        double delaySeconds = 0d)
    {
        var tone = RenderSineCore(frequencyHz, durationSeconds, volume, sampleRate, fadeSeconds, decaySeconds);
        var delaySamples = Math.Max(0, (int)Math.Round(delaySeconds * sampleRate));
        if (delaySamples == 0) { return tone; }

        var delayed = new float[delaySamples + tone.Length];
        Array.Copy(tone, 0, delayed, delaySamples, tone.Length);
        return delayed;
    }

    private static float[] RenderSineCore(double frequencyHz, double durationSeconds, float volume,
        int sampleRate, double fadeSeconds, double? decaySeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        var sampleCount = Math.Max(0, (int)Math.Round(durationSeconds * sampleRate));
        var samples = new float[sampleCount];
        var fadeSamples = Math.Min((int)(fadeSeconds * sampleRate), sampleCount / 2);
        var step = 2d * Math.PI * frequencyHz / sampleRate;

        for (var i = 0; i < sampleCount; i++)
        {
            var envelope = 1d;
            if (fadeSamples > 0)
            {
                if (i < fadeSamples) { envelope = (double)i / fadeSamples; }
                else if (i >= sampleCount - fadeSamples) { envelope = (double)(sampleCount - 1 - i) / fadeSamples; }
            }
            if (decaySeconds is { } decay && decay > 0 && i >= fadeSamples)
            {
                envelope *= Math.Exp(-(i - fadeSamples) / (decay * sampleRate));
            }
            samples[i] = (float)(Math.Sin(step * i) * envelope * volume);
        }
        return samples;
    }
}
