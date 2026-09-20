using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class VertexLayoutData : LayoutData
{
    public double DX { get; set; }
    public double DY { get; set; }
    public double OldDX { get; set; }
    public double OldDY { get; set; }
    public double Mass { get; set; } = 1.0d;
}
