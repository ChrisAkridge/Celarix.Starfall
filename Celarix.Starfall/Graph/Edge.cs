using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class Edge
{
    public long FromVertexId { get; init; }
    public long ToVertexId { get; init; }

    public Edge(long fromVertexId, long toVertexId)
    {
        FromVertexId = fromVertexId;
        ToVertexId = toVertexId;
    }
}
