using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class Vertex
{
    public long Id { get; init; }
    public string Label { get; init; }
    public SPointF Position { get; set; }
    public VertexLayoutData? LayoutData { get; set; }
    public double Size { get; set; } = 1.0d;

    public Vertex(long id, string label, SPointF position)
    {
        Id = id;
        Label = label;
        Position = position;
    }
}
