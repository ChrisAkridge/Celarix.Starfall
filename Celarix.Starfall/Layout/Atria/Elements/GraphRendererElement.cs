using Celarix.Starfall.Graph;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements;

public sealed class GraphRendererElement : AtriaElement
{
    private static readonly SFont _vertexFont = new SFontFamily("Calibri", 24f);

    private readonly Dictionary<long, Vertex> _vertices = new();
    private readonly List<Edge> _edges = new();

    public SPointF CenteredAtGraphPosition { get; set; }

    /// <summary>
    /// Gets or sets the zoom factor for rendering the graph. A value of 1.0 represents no zoom,
    /// values greater than 1.0 represent zooming in, and values less than 1.0 represent zooming out.
    /// </summary>
    public double ZoomFactor { get; set; } = 1.0d;

    public GraphRendererElement(string atriaIdString)
    {
        Id = AtriaId.Parse(atriaIdString);
    }

    public override void Render(IRenderTarget target)
    {
        var scaledFont = _vertexFont.WithSize(_vertexFont.Size * (float)ZoomFactor);

        foreach (var edge in _edges)
        {
            if (_vertices.TryGetValue(edge.FromVertexId, out var fromVertex) && _vertices.TryGetValue(edge.ToVertexId, out var toVertex))
            {
                target.DrawLine(GetScreenPosition(fromVertex.Position), GetScreenPosition(toVertex.Position), new SColor(128, 128, 128, 128), 1f);
            }
        }

        foreach (var vertex in _vertices.Values)
        {
            var screenPosition = GetScreenPosition(vertex.Position);
            var textSize = target.MeasureText(vertex.Label, scaledFont);
            var textBounds = new SRectF(screenPosition.X - textSize.Width / 2, screenPosition.Y - textSize.Height / 2, textSize.Width, textSize.Height);
            var widestDimension = Math.Max(textBounds.Width, textBounds.Height);
            target.DrawEllipse(screenPosition, new SSizeF(widestDimension + 10, widestDimension + 10), new SColor(0, 127, 255, 255), SPaintStyle.Fill);
            target.DrawText(vertex.Label, scaledFont, textBounds, SColor.White, SAngle.Zero);
        }
    }

    public void AddVertex(Vertex vertex)
    {
        _vertices[vertex.Id] = vertex;
    }

    public bool RemoveVertex(Vertex vertex)
    {
        if (_vertices.Remove(vertex.Id))
        {
            _edges.RemoveAll(e => e.FromVertexId == vertex.Id || e.ToVertexId == vertex.Id);
            return true;
        }
        return false;
    }

    public void Connect(Vertex from, Vertex to)
    {
        _edges.Add(new Edge(from.Id, to.Id));
    }

    public void Connect(long fromVertexId, long toVertexId)
    {
        _edges.Add(new Edge(fromVertexId, toVertexId));
    }

    public void Disconnect(Vertex from, Vertex to)
    {
        _edges.RemoveAll(e => (e.FromVertexId == from.Id && e.ToVertexId == to.Id) || (e.FromVertexId == to.Id && e.ToVertexId == from.Id));
    }

    public void Disconnect(long fromVertexId, long toVertexId)
    {
        _edges.RemoveAll(e => (e.FromVertexId == fromVertexId && e.ToVertexId == toVertexId) || (e.FromVertexId == toVertexId && e.ToVertexId == fromVertexId));
    }

    private SPointF GetScreenPosition(SPointF graphPosition)
    {
        var offsetX = CenteredAtGraphPosition.X - (float)(graphPosition.X * ZoomFactor);
        var offsetY = CenteredAtGraphPosition.Y - (float)(graphPosition.Y * ZoomFactor);
        var screenX = offsetX + Bounds.Center.X;
        var screenY = offsetY + Bounds.Center.Y;
        return new SPointF(screenX, screenY);
    }
}
