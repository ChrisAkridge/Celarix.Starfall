using Celarix.Starfall.Atria;
using Celarix.Starfall.Playground.AtriaTests;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Celarix.Starfall.Playground.Presentations
{
    /// <summary>
    /// Renders a fixed number of seconds of a slide straight to a video file through ffmpeg.
    /// Usage: atriaffmpeg [outputPath] [seconds] [h264|prores]
    /// </summary>
    internal static class AtriaFfmpegCurrent
    {
        private const int Width = 1280;
        private const int Height = 720;
        private const double FramesPerSecond = 60;

        public static void Run(string[] args)
        {
            var format = args.Length > 2 && args[2].Equals("prores", StringComparison.OrdinalIgnoreCase)
                ? FfmpegOutputFormat.ProRes4444
                : FfmpegOutputFormat.H264;
            var defaultExtension = format == FfmpegOutputFormat.ProRes4444 ? ".mov" : ".mp4";
            var outputPath = args.Length > 0 ? args[0] : Path.Combine(Environment.CurrentDirectory, "starfall" + defaultExtension);
            var seconds = args.Length > 1 ? double.Parse(args[1]) : 10d;

            var layoutEngine = new AtriaLayoutEngine(Width, Height);
            using var ffmpegTarget = new SkiaFfmpegTarget(new SkiaFfmpegTargetOptions
            {
                Width = Width,
                Height = Height,
                FramesPerSecond = FramesPerSecond,
                OutputPath = outputPath,
                Format = format
            });
            layoutEngine.Attach(ffmpegTarget);

            var slide = new GraphSlide(layoutEngine.Runtime!, new SSizeF(Width, Height));
            layoutEngine.AddSlide(slide, "graph");
            layoutEngine.SetCurrentSlide("graph");

            var totalFrames = (int)Math.Round(seconds * FramesPerSecond);
            var stopwatch = Stopwatch.StartNew();
            for (var frame = 0; frame < totalFrames; frame++)
            {
                layoutEngine.OnFrameRequested(1.0 / FramesPerSecond);
                if (frame % 60 == 0)
                {
                    Console.WriteLine($"\tRendered frame {frame} of {totalFrames}");
                }
            }

            ffmpegTarget.Finish();
            Console.WriteLine($"Wrote {ffmpegTarget.FramesWritten} frames to {outputPath} in {stopwatch.Elapsed.TotalSeconds:F1}s.");
        }
    }
}
