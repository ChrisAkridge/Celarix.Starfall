using Celarix.Starfall.Graph;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests;

public sealed class GraphSlide : AtriaSlide
{
    private static readonly SFont _debugFont = new SFontFamily("Calibri", 24f);
    private bool _enableSpawning = true;
    private long _nextSeed = 1;
    private long? _current;
    private readonly HashSet<long> _seen = [];
    private readonly Random _random = new();

    public GraphSlide(int width, int height) : base(width, height)
    {
    }

    public override void Initialize()
    {
        BackgroundColor = new SColor(8, 0, 130, 255);
        var graphElement = new GraphRendererElement("#graph")
        {
            Size = new SSizeF(1000, 600),
            DrawBounds = true,
            BoundsColor = SColor.Red
        };
        var graphAnchor = new BasisPoint(Center, "#graphAnchor");
        graphElement.AnchorCenterTo(graphAnchor);
        Add([graphElement, graphAnchor]);
    }

    public override void Render(IRenderTarget target)
    {
        base.Render(target);
    }

    public override void Update(double deltaTime)
    {
        var frameNumber = AtriaLayoutEngine.GlobalFrameNumber;

        if (frameNumber % 10 == 0 && _enableSpawning)
        {
            var graph = (GraphRendererElement)Query("#graph").Single();

            if (_current == null)
            {
                // Find the next integer whose trajectory hasn't already
                // been incorporated into the graph.
                while (_seen.Contains(_nextSeed))
                    _nextSeed++;

                AddVertex(graph, _nextSeed);

                _current = _nextSeed;
                _nextSeed++;
            }
            else
            {
                var current = _current.Value;
                var next = current % 2 == 0
                    ? current / 2
                    : current * 3 + 1;

                var alreadySeen = _seen.Contains(next);

                if (!alreadySeen)
                    AddVertex(graph, next);

                graph.Connect(current, next);

                _current = alreadySeen
                    ? null
                    : next;
            }
        }

        base.Update(deltaTime);
    }

    public override void KeyUp(SKeyboardEvent keyboardEvent)
    {
        if (keyboardEvent.Key == SKey.S)
        {
            _enableSpawning = !_enableSpawning;
        }
    }

    private void AddVertex(GraphRendererElement graph, long value)
    {
        var position = new SPointF(
            _random.NextDouble() * 200d,
            _random.NextDouble() * 200d);

        graph.AddVertex(
            new Vertex(value, value.ToString(), position)
            {
                Size = 20d
            });

        _seen.Add(value);
    }
}
