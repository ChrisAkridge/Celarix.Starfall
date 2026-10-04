using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Converters;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    /// <summary>
    /// Renders frames straight into an ffmpeg process as raw video, so no intermediate
    /// PNG files are written. Call <see cref="Finish"/> after the last frame to finalize
    /// the video; <see cref="Dispose"/> also finishes it if that hasn't happened yet.
    /// </summary>
    public sealed class SkiaFfmpegTarget : IRenderTarget, IDisposable
    {
        private const int MaxStandardErrorLines = 40;

        private readonly SkiaFfmpegTargetOptions _options;
        private readonly SKImageInfo _frameInfo;
        private readonly SKBitmap _outputBitmap;
        private readonly Process _ffmpeg;
        private readonly Stream _ffmpegInput;
        private readonly Queue<string> _standardErrorTail = new();

        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private bool _finished;

        public SkiaFfmpegTarget(SkiaFfmpegTargetOptions options)
        {
            if (options.Format == FfmpegOutputFormat.H264 && (options.Width % 2 != 0 || options.Height % 2 != 0))
            {
                throw new ArgumentException($"H.264 output needs an even width and height, but got {options.Width}x{options.Height}.", nameof(options));
            }

            var extension = Path.GetExtension(options.OutputPath);
            if (options.Format == FfmpegOutputFormat.ProRes4444
                && !extension.Equals(".mov", StringComparison.OrdinalIgnoreCase)
                && !extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"ProRes output needs a .mov (or .mkv) file, but the output path is \"{options.OutputPath}\". MP4 can't hold ProRes.", nameof(options));
            }

            _options = options;

            // Skia draws into premultiplied BGRA. ffmpeg's bgra input expects straight alpha,
            // so frames are converted on the way out; for opaque frames the bytes are identical.
            _frameInfo = new SKImageInfo(options.Width, options.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _outputBitmap = new SKBitmap(new SKImageInfo(options.Width, options.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul));

            _bitmap = new SKBitmap(_frameInfo);
            _canvas = new SKCanvas(_bitmap);

            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath));
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            _ffmpeg = StartFfmpeg(options);
            _ffmpegInput = _ffmpeg.StandardInput.BaseStream;

            SkiaTextRendering.SetShaperCacheDuration(30000);
        }

        public bool CanAnimate => true;

        public bool IsAnimating { get; set; }

        /// <summary>
        /// Gets the number of frames written to ffmpeg so far.
        /// </summary>
        public int FramesWritten { get; private set; }

        public void Clear(SColor color) => SkiaCommon.Clear(_canvas, color);

        public void PushTransform(STransform2D transform) => SkiaCommon.PushTransform(_canvas, transform);

        public void PopTransform() => SkiaCommon.PopTransform(_canvas);

        public void Complete()
        {
            ObjectDisposedException.ThrowIf(_finished, this);

            _canvas.Flush();
            if (!_bitmap.PeekPixels().ReadPixels(_outputBitmap.PeekPixels()))
            {
                throw new InvalidOperationException("Could not read the rendered frame from the Skia bitmap.");
            }

            try
            {
                _ffmpegInput.Write(_outputBitmap.GetPixelSpan());
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"ffmpeg stopped accepting frames.{Environment.NewLine}{GetStandardErrorTail()}", ex);
            }

            FramesWritten += 1;

            // Start every frame from a fresh, transparent canvas, matching SkiaPngTarget.
            _canvas.Dispose();
            _bitmap.Erase(SKColors.Transparent);
            _canvas = new SKCanvas(_bitmap);
        }

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation) =>
            SkiaCommon.DrawRectangle(_canvas, bounds, color, paintStyle, rotation);

        public void DrawEllipse(SPointF center, SSizeF size, SColor color, SPaintStyle paintStyle) =>
            SkiaCommon.DrawEllipse(_canvas, center, size, color, paintStyle);

        public void DrawRadialGradientCircle(SPointF center, double radius, SColor[] colors, double[] colorPositions, SShaderTileMode tileMode, SBlendMode blendMode) =>
            SkiaCommon.DrawRadialGradientCircle(_canvas, center, radius, colors, colorPositions, tileMode, blendMode);

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center) =>
            SkiaCommon.DrawText(_canvas, text, font, bounds, color, rotation, alignment);

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation) =>
            SkiaCommon.DrawTextDirectly(_canvas, text, font, bounds, color, rotation);

        public void DrawLine(SPointF start, SPointF end, SColor color, float thickness) =>
            SkiaCommon.DrawLine(_canvas, start, end, color, thickness);

        public void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation) =>
            SkiaCommon.DrawImageFromFile(_canvas, filePath, bounds, opacity, rotation);

        public void DrawImage(SImage image, SRectF bounds, double opacity = 1d, SAngle? rotation = null) =>
            SkiaCommon.DrawImage(_canvas, image, bounds, opacity, rotation ?? SAngle.Zero);

        public void DrawCroppedImage(SImage image, SRectF sourceRect, SRectF destRect, double opacity = 1d) =>
            SkiaCommon.DrawCroppedImage(_canvas, image, sourceRect, destRect, opacity);

        public void DrawPoint(SPointF point, SColor color) =>
            SkiaCommon.DrawPoint(_canvas, point, color);

        public void DrawPath(IEnumerable<SPathCommand> pathCommands, SPathStyle pathStyle) =>
            SkiaCommon.DrawPath(_canvas, pathCommands, pathStyle);

        public float FitTextToHeight(string text, SFont font, float height) =>
            SkiaTextRendering.FitTextToHeight(text, font, height);

        public float FitTextToWidth(string text, SFont font, float width) =>
            SkiaTextRendering.FitTextToWidth(text, font, width);

        public SSizeF MeasureText(string text, SFont font) => SkiaTextRendering.GetFont(font).MeasureShapedText(text);

        public SFontMetrics GetFontMetrics(SFont font)
        {
            var f = SkiaTextRendering.GetFont(font).GetFontMetrics(out var skMetrics);
            return new SFontMetrics(skMetrics.Ascent, skMetrics.Descent, skMetrics.Leading);
        }

        public IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size) => new SkiaOffscreenTarget((int)size.Width, (int)size.Height);

        public void Start() { }

        /// <summary>
        /// Closes ffmpeg's input, waits for it to write the video, and throws if ffmpeg failed.
        /// </summary>
        public void Finish()
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            _ffmpegInput.Dispose();
            _ffmpeg.WaitForExit();

            if (_ffmpeg.ExitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg exited with code {_ffmpeg.ExitCode}.{Environment.NewLine}{GetStandardErrorTail()}");
            }
        }

        public void Dispose()
        {
            try
            {
                Finish();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: {ex.Message}");
            }
            finally
            {
                _ffmpeg.Dispose();
                _canvas.Dispose();
                _bitmap.Dispose();
                _outputBitmap.Dispose();
            }
        }

        private Process StartFfmpeg(SkiaFfmpegTargetOptions options)
        {
            var startInfo = new ProcessStartInfo(options.FfmpegPath)
            {
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var argument in BuildArguments(options))
            {
                startInfo.ArgumentList.Add(argument);
            }

            Process? process;
            try
            {
                process = Process.Start(startInfo);
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException($"Could not start ffmpeg at \"{options.FfmpegPath}\". Install it or set {nameof(SkiaFfmpegTargetOptions.FfmpegPath)}.", ex);
            }

            if (process == null)
            {
                throw new InvalidOperationException($"Could not start ffmpeg at \"{options.FfmpegPath}\".");
            }

            // ffmpeg logs continuously to stderr; it has to be drained or ffmpeg blocks.
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                lock (_standardErrorTail)
                {
                    _standardErrorTail.Enqueue(e.Data);
                    while (_standardErrorTail.Count > MaxStandardErrorLines)
                    {
                        _standardErrorTail.Dequeue();
                    }
                }
            };
            process.BeginErrorReadLine();

            return process;
        }

        private static IEnumerable<string> BuildArguments(SkiaFfmpegTargetOptions options)
        {
            var frameRate = options.FramesPerSecond.ToString(CultureInfo.InvariantCulture);

            string[] input =
            [
                "-hide_banner", "-loglevel", "warning", "-y",
                "-f", "rawvideo",
                "-pix_fmt", "bgra",
                "-s", $"{options.Width}x{options.Height}",
                "-framerate", frameRate,
                "-i", "-"
            ];

            string[] encoding = options.Format switch
            {
                FfmpegOutputFormat.H264 =>
                [
                    "-c:v", "libx264",
                    "-preset", "medium",
                    "-crf", "18",
                    "-pix_fmt", "yuv420p",
                    "-movflags", "+faststart"
                ],
                FfmpegOutputFormat.ProRes4444 =>
                [
                    "-c:v", "prores_ks",
                    "-profile:v", "4444",
                    "-pix_fmt", "yuva444p10le",
                    "-vendor", "apl0"
                ],
                _ => throw new ArgumentOutOfRangeException(nameof(options), $"Unknown ffmpeg output format {options.Format}.")
            };

            return [.. input, .. encoding, options.OutputPath];
        }

        private string GetStandardErrorTail()
        {
            lock (_standardErrorTail)
            {
                return _standardErrorTail.Count == 0
                    ? "ffmpeg wrote nothing to standard error."
                    : "ffmpeg said:" + Environment.NewLine + string.Join(Environment.NewLine, _standardErrorTail);
            }
        }
    }
}
