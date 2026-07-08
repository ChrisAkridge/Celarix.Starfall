using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public sealed class SMonitorInfo
    {
        public int MonitorIndex { get; }
        public int Width { get; }
        public int Height { get; }
        public string Name { get; }
        public IntPtr Handle { get; }
        public SRectF WorkArea { get; }

        public SMonitorInfo(int monitorIndex,
            int width,
            int height,
            string name,
            IntPtr handle,
            SRectF workArea)
        {
            MonitorIndex = monitorIndex;
            Width = width;
            Height = height;
            Name = name;
            Handle = handle;
            WorkArea = workArea;
        }
    }
}
