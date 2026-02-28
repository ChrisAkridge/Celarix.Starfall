using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public sealed class RenderOptions
    {
        public required int Width { get; init; }
        public required int Height { get; init; }
        public string? OutputPath { get; init; }
        public string? OutputFileNamePattern { get; init; }

        public RenderOptions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
