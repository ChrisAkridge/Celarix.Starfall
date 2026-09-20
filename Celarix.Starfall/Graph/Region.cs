using Celarix.Starfall.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public sealed class Region
{
    private readonly List<Vertex> _vertices = [];
    private readonly List<Region> _subregions = [];

    public double Mass { get; private set; }
    public double MassCenterX { get; private set; }
    public double MassCenterY { get; private set; }
    public double Size { get; private set; }

    public Region(IEnumerable<Vertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices, nameof(vertices));
        _vertices.AddRange(vertices);
        UpdateMassAndGeometry();
    }

    public Region(IList<Vertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices, nameof(vertices));
        _vertices = [.. vertices];
        UpdateMassAndGeometry();
    }

    private void UpdateMassAndGeometry()
    {
        if (_vertices.Count > 1)
        {
            Mass = 0.0d;
            var massSumX = 0d;
            var massSumY = 0d;

            foreach (var vertex in _vertices)
            {
                var layout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is missing.");
                Mass += layout.Mass;
                massSumX += vertex.Position.X * layout.Mass;
                massSumY += vertex.Position.Y * layout.Mass;
            }

            MassCenterX = massSumX / Mass;
            MassCenterY = massSumY / Mass;

            Size = double.MinValue;
            foreach (var vertex in _vertices)
            {
                double x0 = vertex.Position.X - MassCenterX;
                double y0 = vertex.Position.Y - MassCenterY;
                var distance = Math.Sqrt((x0 * x0) + (y0 * y0));
                Size = Math.Max(Size, distance * 2.0d);
            }
        }
    }

    public void BuildSubregions()
    {
        if (_vertices.Count > 1)
        {
            var leftVertices = new List<Vertex>();
            var rightVertices = new List<Vertex>();
            foreach (var vertex in _vertices)
            {
                var verticesColumn = (vertex.Position.X < MassCenterX)
                    ? leftVertices
                    : rightVertices;
                verticesColumn.Add(vertex);
            }

            var topLeftVertices = new List<Vertex>();
            var bottomLeftVertices = new List<Vertex>();

            foreach (var leftVertex in leftVertices)
            {
                var verticesLine = (leftVertex.Position.Y < MassCenterY)
                    ? topLeftVertices
                    : bottomLeftVertices;
                verticesLine.Add(leftVertex);
            }

            var bottomRightVertices = new List<Vertex>();
            var topRightVertices = new List<Vertex>();
            foreach (var rightVertex in rightVertices)
            {
                var verticesLine = (rightVertex.Position.Y < MassCenterY)
                    ? topRightVertices
                    : bottomRightVertices;
                verticesLine.Add(rightVertex);
            }

            AddSubregionOrIndividualVertices(topLeftVertices);
            AddSubregionOrIndividualVertices(bottomLeftVertices);
            AddSubregionOrIndividualVertices(bottomRightVertices);
            AddSubregionOrIndividualVertices(topRightVertices);

            foreach (var subregion in _subregions)
            {
                subregion.BuildSubregions();
            }
        }
    }

    public void ApplyForce(Vertex vertex, RepulsionForce force, double theta)
    {
        if (_vertices.Count < 2)
        {
            var regionVertex = _vertices[0];
            force.Apply(vertex, regionVertex);
        }
        else
        {
            var distance = MathHelpers.Distance(vertex.Position.X - MassCenterX, vertex.Position.Y - MassCenterY);
            if (distance * theta > Size)
            {
                force.Apply(vertex, this);
            }
            else
            {
                foreach (var subregion in _subregions)
                {
                    subregion.ApplyForce(vertex, force, theta);
                }
            }
        }
    }

    private void AddSubregionOrIndividualVertices(IReadOnlyList<Vertex> vertices)
    {
        if (vertices.Count == 0)
        {
            return;
        }

        if (vertices.Count < _vertices.Count)
        {
            _subregions.Add(new Region(vertices));
            return;
        }

        foreach (var vertex in vertices)
        {
            _subregions.Add(new Region([vertex]));
        }
    }
}
