namespace Celarix.Starfall.Audio;

/// <summary>
/// Renders sine-wave tones into sample buffers. Platform-independent, so it can feed live
/// playback or an offline audio track alike.
/// </summary>
public static class ToneSynth
{
    public const int DefaultSampleRate = 44100;

    /// <summary>
    /// Renders a mono sine tone with the given envelope, after <paramref name="delaySeconds"/> of silence.
    /// </summary>
    /// <param name="shape">The volume envelope; null uses <see cref="ToneShape.Default"/>. Fades are clamped to half the tone.</param>
    public static float[] RenderSine(double frequencyHz, double durationSeconds, float volume = 0.3f,
        int sampleRate = DefaultSampleRate, ToneShape? shape = null, double delaySeconds = 0d)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        var envelopeShape = shape ?? ToneShape.Default;
        var delaySamples = Math.Max(0, (int)Math.Round(delaySeconds * sampleRate));
        var sampleCount = Math.Max(0, (int)Math.Round(durationSeconds * sampleRate));
        var samples = new float[delaySamples + sampleCount];
        var fadeIn = Math.Min((int)Math.Round(envelopeShape.FadeInSeconds * sampleRate), sampleCount / 2);
        var fadeOut = Math.Min((int)Math.Round(envelopeShape.FadeOutSeconds * sampleRate), sampleCount / 2);
        var step = 2d * Math.PI * frequencyHz / sampleRate;

        for (var i = 0; i < sampleCount; i++)
        {
            var envelope = 1d;
            if (i < fadeIn)
            {
                envelope = Ramp((i + 0.5d) / fadeIn, envelopeShape.Smooth);
            }
            else if (i >= sampleCount - fadeOut)
            {
                envelope = Ramp((sampleCount - i - 0.5d) / fadeOut, envelopeShape.Smooth);
            }
            if (envelopeShape.DecaySeconds is { } decay && decay > 0 && i >= fadeIn)
            {
                envelope *= Math.Exp(-(i - fadeIn) / (decay * sampleRate));
            }
            samples[delaySamples + i] = (float)(Math.Sin(step * i) * envelope * volume);
        }
        return samples;
    }

    /// <summary>
    /// A fade from 0 to 1 over <paramref name="t"/> in [0, 1]: straight, or a raised cosine whose
    /// fade-in and matching fade-out always sum to 1.
    /// </summary>
    private static double Ramp(double t, bool smooth) => smooth ? 0.5d - 0.5d * Math.Cos(Math.PI * t) : t;
}
