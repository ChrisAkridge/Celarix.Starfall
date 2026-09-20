using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class Edge
{
    public double Weight { get; set; } = 1.0d;

    public long FromVertexId { get; init; }
    public long ToVertexId { get; init; }

    public Edge(long fromVertexId, long toVertexId)
    {
        FromVertexId = fromVertexId;
        ToVertexId = toVertexId;
    }
}
