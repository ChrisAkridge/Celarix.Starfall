using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Audio
{
    public sealed class SoundPlayer : IDisposable
    {
        private readonly Dictionary<string, CachedSound> _soundCache = new();
        private readonly List<IWavePlayer> _activePlayers = new();

        public void LoadSound(string name, string filePath)
        {
            if (!_soundCache.ContainsKey(name))
            {
                var cachedSound = new CachedSound(filePath);
                _soundCache[name] = cachedSound;
            }
        }

        public void PlaySound(string name, float volume = 1.0f)
        {
            if (!_soundCache.TryGetValue(name, out var sound))
                return;

            // Create a new output device for this sound
            var outputDevice = new WaveOutEvent();
            var provider = new CachedSoundSampleProvider(sound) { Volume = volume };

            outputDevice.Init(provider);

            // Remove from list when playback stops
            outputDevice.PlaybackStopped += (sender, args) =>
            {
                outputDevice.Dispose();
                _activePlayers.Remove(outputDevice);
            };

            _activePlayers.Add(outputDevice);
            outputDevice.Play(); // Non-blocking!
        }

        public void Dispose()
        {
            foreach (var player in _activePlayers)
            {
                player?.Dispose();
            }

            _activePlayers.Clear();
        }
    }

    internal class CachedSound
    {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }

        public CachedSound(string audioFilePath)
        {
            using var audioFileReader = new AudioFileReader(audioFilePath);
            WaveFormat = audioFileReader.WaveFormat;

            var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
            var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer[..samplesRead]);
            }
            AudioData = wholeFile.ToArray();
        }
    }

    internal class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _cachedSound;
        private long _position;

        public float Volume { get; set; } = 1.0f;
        public WaveFormat WaveFormat => _cachedSound.WaveFormat;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            _cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = _cachedSound.AudioData.Length - _position;
            var samplesToCopy = Math.Min(availableSamples, count);

            for (int i = 0; i < samplesToCopy; i++)
            {
                buffer[offset + i] = _cachedSound.AudioData[_position++] * Volume;
            }

            return (int)samplesToCopy;
        }
    }
}
