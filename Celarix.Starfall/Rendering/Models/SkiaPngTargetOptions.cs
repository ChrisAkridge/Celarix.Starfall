using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public sealed class SkiaPngTargetOptions
    {
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required string OutputPath { get; init; }
        public required double FramesPerSecond { get; init; }

        public double FrameDuration => 1.0 / FramesPerSecond;
    }
}
