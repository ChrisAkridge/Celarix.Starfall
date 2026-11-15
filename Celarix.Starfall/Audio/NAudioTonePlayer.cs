using System.Runtime.Versioning;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Celarix.Starfall.Audio;

/// <summary>
/// Plays tones through the default output device with NAudio. Windows only: NAudio's output
/// devices use the Windows audio APIs.
/// </summary>
/// <remarks>
/// One output device stays open with a mixer feeding it, so tones overlap cheaply instead of
/// opening a device per tone.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class NAudioTonePlayer : ITonePlayer, IDisposable
{
    private readonly WaveOutEvent _output;
    private readonly MixingSampleProvider _mixer;

    public NAudioTonePlayer(int sampleRate = ToneSynth.DefaultSampleRate)
    {
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1))
        {
            // Keep reading silence when no tones are playing, so the device never stops.
            ReadFully = true
        };
        _output = new WaveOutEvent { DesiredLatency = 60 };
        _output.Init(_mixer);
        _output.Play();
    }

    public void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f,
        double delaySeconds = 0d, double? decaySeconds = null)
    {
        var samples = ToneSynth.RenderSine(frequencyHz, durationSeconds, volume, _mixer.WaveFormat.SampleRate,
            decaySeconds: decaySeconds, delaySeconds: delaySeconds);
        _mixer.AddMixerInput(new BufferSampleProvider(samples, _mixer.WaveFormat));
    }

    public void Dispose()
    {
        _output.Stop();
        _output.Dispose();
    }

    private sealed class BufferSampleProvider(float[] samples, WaveFormat format) : ISampleProvider
    {
        private int _position;

        public WaveFormat WaveFormat => format;

        public int Read(float[] buffer, int offset, int count)
        {
            var copied = Math.Min(count, samples.Length - _position);
            Array.Copy(samples, _position, buffer, offset, copied);
            _position += copied;
            return copied;
        }
    }
}
