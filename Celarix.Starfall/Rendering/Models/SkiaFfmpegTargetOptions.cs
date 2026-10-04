using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public enum FfmpegOutputFormat
    {
        /// <summary>
        /// H.264 in an MP4 container. Opaque, widely playable, small files. Requires even width and height.
        /// </summary>
        H264,

        /// <summary>
        /// ProRes 4444 in a QuickTime container. Keeps the alpha channel, for overlays in video editors.
        /// </summary>
        ProRes4444
    }

    public sealed class SkiaFfmpegTargetOptions
    {
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required string OutputPath { get; init; }
        public required double FramesPerSecond { get; init; }

        public FfmpegOutputFormat Format { get; init; } = FfmpegOutputFormat.H264;

        /// <summary>
        /// The ffmpeg executable to run. Defaults to "ffmpeg", which is resolved from PATH.
        /// </summary>
        public string FfmpegPath { get; init; } = "ffmpeg";

        public double FrameDuration => 1.0 / FramesPerSecond;
    }
}
