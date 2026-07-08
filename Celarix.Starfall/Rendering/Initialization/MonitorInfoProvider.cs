using Celarix.Starfall.Rendering.Models;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Initialization
{
    public static class MonitorInfoProvider
    {
        public static IReadOnlyList<SMonitorInfo> GetMonitorInfos()
        {
            var monitors = Monitors.GetMonitors();
            return [.. monitors.Select((monitor, index) => {
                var workArea = monitor.WorkArea;
                var workAreaRect = new SRectF(workArea.Min.X, workArea.Min.Y, workArea.Max.X - workArea.Min.X, workArea.Max.Y - workArea.Min.Y);

                return new SMonitorInfo(index,
                    monitor.HorizontalResolution,
                    monitor.VerticalResolution,
                    monitor.Name,
                    monitor.Handle.Pointer,
                    workAreaRect);
            })];
        }
    }
}
