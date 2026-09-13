using Celarix.Starfall.Graph;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests;

public sealed class GraphSlide : AtriaSlide
{
    public GraphSlide(int width, int height) : base(width, height)
    {
    }

    public override void Initialize()
    {
        BackgroundColor = new SColor(8, 0, 130, 255);
        var graphElement = new GraphRendererElement("#graph")
        {
            Size = new SSizeF(1000, 600)
        };
        var graphAnchor = new BasisPoint(Center, "#graphAnchor");
        graphElement.AnchorCenterTo(graphAnchor);
        Add([graphElement, graphAnchor]);

        graphElement.AddVertex(new Vertex(0L, "1", SPointF.Zero));
        graphElement.AddVertex(new Vertex(1L, "2", new SPointF(200, 0)));
        graphElement.AddVertex(new Vertex(2L, "4", new SPointF(100, 100)));
        graphElement.Connect(0L, 1L);
        graphElement.Connect(1L, 2L);
        graphElement.Connect(2L, 0L);
    }
}
