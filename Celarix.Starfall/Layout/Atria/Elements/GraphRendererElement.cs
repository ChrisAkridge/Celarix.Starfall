using AngleSharp.Dom;
using Celarix.Starfall.Graph;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements;

public sealed class GraphRendererElement : AtriaElement
{
    // ForceAtlas2 implementation from https://github.com/gephi/gephi/blob/master/modules/LayoutPlugin/src/main/java/org/gephi/layout/plugin/forceAtlas2/ForceAtlas2.java
    private static readonly SFont _vertexFont = new SFontFamily("Calibri", 16f);

    private readonly Dictionary<long, Vertex> _vertices = new();
    private readonly List<Edge> _edges = new();
    private Region? _rootRegion;

    // okay
    // this is an EXCEPTIONALLY dumb way to do this
    // but I think it's funny
    private long _initializationSeed = ((Func<(Random Random, int High), long>)(rh =>
    {
        unchecked
        {
            var low = rh.Random.Next(int.MinValue, int.MaxValue);
            ulong ulow = (uint)low;
            ulong uhigh = (uint)rh.High;
            ulong uresult = (uhigh << 32) | ulow;
            return (long)uresult;
        }
    }))(((Func<(Random Random, int High)>)(() =>
    {
        var random = new Random();
        var high = random.Next(int.MinValue, int.MaxValue);
        // wait what do you MEAN there's a Random.NextInt64()?
        return (random, high);
    }))());

    public SPointF CenteredAtGraphPosition { get; set; }
    public bool DrawBounds { get; set; }
    public SColor BoundsColor { get; set; }

    /// <summary>
    /// Gets or sets the zoom factor for rendering the graph. A value of 1.0 represents no zoom,
    /// values greater than 1.0 represent zooming in, and values less than 1.0 represent zooming out.
    /// </summary>
    public double ZoomFactor { get; set; } = 1.0d;

    public double EdgeWeightInfluence { get; set; }
    public double JitterTolerance { get; set; }
    public double ScalingRatio { get; set; }
    public double Gravity { get; set; }
    public double Speed { get; set; }
    public double SpeedEfficiency { get; set; }
    public bool LinLogMode { get; set; }
    public bool OutboundAttractionDistribution { get; set; }
    public bool AdjustSizes { get; set; }
    public bool BarnesHutOptimize { get; set; }
    public double BarnesHutTheta { get; set; }
    public bool NormalizeEdgeWeights { get; set; }
    public bool StrongGravityMode { get; set; }
    public bool InvertedEdgeWeightsMode { get; set; }

    public GraphRendererElement(string atriaIdString)
    {
        Id = AtriaId.Parse(atriaIdString);

        // Initialize as much as we can like in initAlgo()
        Speed = 1.0d;
        SpeedEfficiency = 1.0d;
    }

    public override void Render(IRenderTarget target)
    {
        var scaledFont = _vertexFont.WithSize(_vertexFont.Size * (float)ZoomFactor);

        if (DrawBounds)
        {
            target.DrawRectangle(Bounds, BoundsColor, SPaintStyle.Stroke, SAngle.Zero);
        }

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
            var circleSize = vertex.Size;
            var textBounds = new SRectF(screenPosition.X - textSize.Width / 2, screenPosition.Y - textSize.Height / 2, textSize.Width, textSize.Height);
            target.DrawEllipse(screenPosition, new SSizeF(circleSize, circleSize), new SColor(0, 127, 255, 255), SPaintStyle.Fill);
            target.DrawText(vertex.Label, scaledFont, textBounds, SColor.White, SAngle.Zero);
        }
    }

    public override void Update(FrameTime frameTime)
    {
        // Going to assume this is the goAlgo equivalent.
        var isDynamicWeight = true; // This is a placeholder. The original code pulls it from the graph's edge table properties.

        foreach (var vertex in _vertices.Values)
        {
            vertex.LayoutData ??= new VertexLayoutData();
            vertex.LayoutData.Mass = 1 + GetDegree(vertex);
            vertex.LayoutData.OldDX = vertex.LayoutData.DX;
            vertex.LayoutData.OldDY = vertex.LayoutData.DY;
            vertex.LayoutData.DX = 0d;
            vertex.LayoutData.DY = 0d;
        }

        if (BarnesHutOptimize)
        {
            _rootRegion = new Region(_vertices.Values);
            _rootRegion.BuildSubregions();
        }

        // Compensate if OutboundAttractionDistribution is enabled
        var outboundAttractionCompensation = 1d;

        if (OutboundAttractionDistribution)
        {
            outboundAttractionCompensation = 0d;

            foreach (var vertex in _vertices.Values)
            {
                var layout = vertex.LayoutData!;
                outboundAttractionCompensation += layout.Mass;
            }

            outboundAttractionCompensation /= _vertices.Count;
        }

        var repulsionForce = new ForceFactory().BuildRepulsion(AdjustSizes, ScalingRatio);
        var gravityForce = StrongGravityMode
            ? new StrongGravity(ScalingRatio)
            : repulsionForce;
        RunRepulsion([.. _vertices.Values], BarnesHutOptimize, BarnesHutTheta, Gravity,
            gravityForce, ScalingRatio, _rootRegion, repulsionForce);

        // Attraction
        var attraction = new ForceFactory().BuildAttraction(
            LinLogMode,
            OutboundAttractionDistribution,
            AdjustSizes,
            OutboundAttractionDistribution
                ? outboundAttractionCompensation
                : 1d);
        if (EdgeWeightInfluence == 0d)
        {
            foreach (var edge in _edges)
            {
                attraction.Apply(_vertices[edge.FromVertexId], _vertices[edge.ToVertexId], 1);
            }
        }
        else if (EdgeWeightInfluence == 1d)
        {
            if (NormalizeEdgeWeights)
            {
                double w;
                var edgeWeightMin = double.MaxValue;
                var edgeWeightMax = double.MinValue;
                foreach (var edge in _edges)
                {
                    w = GetEdgeWeight(edge, isDynamicWeight, 1); // 1 since Update() is called once per frame
                    edgeWeightMin = Math.Min(w, edgeWeightMin);
                    edgeWeightMax = Math.Max(w, edgeWeightMax);
                }

                if (edgeWeightMin < edgeWeightMax)
                {
                    foreach (var edge in _edges)
                    {
                        w = (GetEdgeWeight(edge, isDynamicWeight, 1) - edgeWeightMin) / (edgeWeightMax - edgeWeightMin);
                        attraction.Apply(_vertices[edge.FromVertexId], _vertices[edge.ToVertexId], w);
                    }
                }
                else
                {
                    foreach (var edge in _edges)
                    {
                        attraction.Apply(_vertices[edge.FromVertexId], _vertices[edge.ToVertexId], 1);
                    }
                }
            }
            else
            {
                foreach (var edge in _edges)
                {
                    attraction.Apply(_vertices[edge.FromVertexId],
                        _vertices[edge.ToVertexId],
                        GetEdgeWeight(edge, isDynamicWeight, 1));
                }
            }
        }
        else
        {
            if (NormalizeEdgeWeights)
            {
                double w;
                var edgeWeightMin = double.MaxValue;
                var edgeWeightMax = double.MinValue;
                foreach (var edge in _edges)
                {
                    w = GetEdgeWeight(edge, isDynamicWeight, 1); // 1 since Update() is called once per frame
                    edgeWeightMin = Math.Min(w, edgeWeightMin);
                    edgeWeightMax = Math.Max(w, edgeWeightMax);
                }

                if (edgeWeightMin < edgeWeightMax)
                {
                    foreach (var edge in _edges)
                    {
                        w = (GetEdgeWeight(edge, isDynamicWeight, 1) - edgeWeightMin) / (edgeWeightMax - edgeWeightMin);
                        attraction.Apply(_vertices[edge.FromVertexId], _vertices[edge.ToVertexId], Math.Pow(w, EdgeWeightInfluence));
                    }
                }
                else
                {
                    foreach (var edge in _edges)
                    {
                        attraction.Apply(_vertices[edge.FromVertexId], _vertices[edge.ToVertexId], 1);
                    }
                }
            }
            else
            {
                foreach (var edge in _edges)
                {
                    attraction.Apply(_vertices[edge.FromVertexId],
                        _vertices[edge.ToVertexId],
                        Math.Pow(GetEdgeWeight(edge, isDynamicWeight, 1), EdgeWeightInfluence));
                }
            }
        }

        // Auto adjust speed
        var totalSwinging = 0d; // How much irregular movement
        var totalEffectiveTraction = 0d; // How much useful movement
        foreach (var vertex in _vertices.Values)
        {
            var layout = vertex.LayoutData;

            // The Gephi code doesn't check for null here, so we won't either
            // It's dumb, but if they wanted it to blow up on a null reference, who am I to say otherwise

            if (!layout.Fixed)
            {
                var swinging = Math.Sqrt(Math.Pow(layout.OldDX - layout.DX, 2) + Math.Pow(layout.OldDY - layout.DY, 2));
                totalSwinging += layout.Mass * swinging; // If the node has a burst change of direction, then it's not converging.
                totalEffectiveTraction += layout.Mass
                    * 0.5d
                    * Math.Sqrt(Math.Pow(layout.OldDX + layout.DX, 2) + Math.Pow(layout.OldDY + layout.DY, 2));
            }
        }

        // Early exit if the layout is basically converged. Avoids a NaN in the speed calculation when totalSwinging is 0.
        const double epsilon = 1e-12;

        if (totalSwinging < epsilon && totalEffectiveTraction < epsilon)
        {
            return;
        }

        // We want that swingingMovement < tolerance * convergenceMovement

        // Optimize jitter tolerance
        // The 'right' jitter tolerance for this network. Bigger networks need more tolerance. Denser networks need less tolerance. Totally empiric.
        var estimatedOptimalJitterTolerance = 0.05d * Math.Sqrt(_vertices.Count);
        var minJitterTolerance = Math.Sqrt(estimatedOptimalJitterTolerance);
        var maxJitterTolerance = 10d;
        var jitterTolerance = JitterTolerance * Math.Max(minJitterTolerance,
            Math.Min(maxJitterTolerance, estimatedOptimalJitterTolerance * totalEffectiveTraction / Math.Pow(_vertices.Count, 2)));

        var minSpeedEfficiency = 0.05d;

        // Protection against erractic behavior
        if (totalSwinging / totalEffectiveTraction > 2.0d)
        {
            if (SpeedEfficiency > minSpeedEfficiency)
            {
                SpeedEfficiency *= 0.5d;
            }
            jitterTolerance = Math.Max(jitterTolerance, JitterTolerance);
        }

        const double minSpeed = 1e-3;
        var targetSpeed =
            totalSwinging < epsilon
                ? double.PositiveInfinity
                : jitterTolerance
                    * SpeedEfficiency
                    * totalEffectiveTraction
                    / totalSwinging;

        // Speed efficiency is how the speed really corresponds to the swinging vs. convergence tradeoff
        // We adjust it slowly and carefully
        if (totalSwinging > jitterTolerance * totalEffectiveTraction)
        {
            if (SpeedEfficiency > minSpeedEfficiency)
            {
                SpeedEfficiency *= 0.7d;
            }
        }
        else if (Speed < 1000d)
        {
            SpeedEfficiency *= 1.3d;
        }

        // But the speed shoudn't rise too much too quickly, since it would make the convergence drop dramatically.
        var maxRise = 0.5d;
        Speed = Math.Max(
            minSpeed,
            Speed + Math.Min(targetSpeed - Speed, maxRise * Speed));

        if (targetSpeed == 0d)
        {
            Debug.WriteLine(
                $"ZERO TARGET SPEED: " +
                $"swing={totalSwinging:R} " +
                $"traction={totalEffectiveTraction:R} " +
                $"jt={jitterTolerance:R} " +
                $"eff={SpeedEfficiency:R} " +
                $"speed={Speed:R}");
        }

        // Apply forces
        var graphTopLeft = GetGraphPosition(
            new SPointF(Bounds.Left, Bounds.Top));

        var graphBottomRight = GetGraphPosition(
            new SPointF(Bounds.Right, Bounds.Bottom));

        if (AdjustSizes)
        {
            // If nodes overlap prevention is active, it's not possible to trust the swinging mesure.
            foreach (var vertex in _vertices.Values)
            {
                var layout = vertex.LayoutData;
                if (!layout.Fixed)
                {
                    var swinging = layout.Mass * Math.Sqrt((layout.OldDX - layout.DX) * (layout.OldDX - layout.DX) + (layout.OldDY - layout.DY) * (layout.OldDY - layout.DY));
                    var factor = 0.1d * Speed / (1d + Math.Sqrt(Speed * swinging));
                    var df = Math.Sqrt(Math.Pow(layout.DX, 2) + Math.Pow(layout.DY, 2));
                    factor = df == 0d ? 0d : Math.Min(factor * df, 10d) / df;

                    var x = vertex.Position.X + layout.DX * factor;
                    var y = vertex.Position.Y + layout.DY * factor;

                    vertex.Position = new SPointF(
                        Math.Max(graphTopLeft.X + vertex.Size, Math.Min(graphBottomRight.X - vertex.Size, x)),
                        Math.Max(graphTopLeft.Y + vertex.Size, Math.Min(graphBottomRight.Y - vertex.Size, y))
                    );
                }
            }
        }
        else
        {
            foreach (var vertex in _vertices.Values)
            {
                var layout = vertex.LayoutData;
                if (!layout.Fixed)
                {

                    // Adaptive auto-speed: the speed of each node is lowered
                    // when the node swings.
                    double swinging = layout.Mass * Math.Sqrt(
                        (layout.OldDX - layout.DX) * (layout.OldDX - layout.DX) +
                            (layout.OldDY - layout.DY) * (layout.OldDY - layout.DY));
                    //double factor = speed / (1f + Math.sqrt(speed * swinging));
                    double factor = Speed / (1f + Math.Sqrt(Speed * swinging));

                    double x = vertex.Position.X + layout.DX * factor;
                    double y = vertex.Position.Y + layout.DY * factor;

                    var radius = vertex.Size;

                    var minX = graphTopLeft.X + radius;
                    var maxX = graphBottomRight.X - radius;
                    var minY = graphTopLeft.Y + radius;
                    var maxY = graphBottomRight.Y - radius;

                    vertex.Position = new SPointF(
                        Math.Max(minX, Math.Min(maxX, x)),
                        Math.Max(minY, Math.Min(maxY, y))
                    );
                }
            }
        }
    }

    public void AddVertex(Vertex vertex)
    {
        _vertices[vertex.Id] = vertex;
        ResetProperties();
    }

    public bool RemoveVertex(Vertex vertex)
    {
        if (_vertices.Remove(vertex.Id))
        {
            _edges.RemoveAll(e => e.FromVertexId == vertex.Id || e.ToVertexId == vertex.Id);
            ResetProperties();
            return true;
        }
        return false;
    }

    public void Connect(Vertex from, Vertex to)
    {
        _edges.Add(new Edge(from.Id, to.Id));
        ResetProperties();
    }

    public void Connect(long fromVertexId, long toVertexId)
    {
        if (!_edges.Any(e => e.FromVertexId == fromVertexId && e.ToVertexId == toVertexId))
        {
            _edges.Add(new Edge(fromVertexId, toVertexId));
            ResetProperties();
        }
    }

    public void Disconnect(Vertex from, Vertex to)
    {
        _edges.RemoveAll(e => (e.FromVertexId == from.Id && e.ToVertexId == to.Id) || (e.FromVertexId == to.Id && e.ToVertexId == from.Id));
        ResetProperties();
    }

    public void Disconnect(long fromVertexId, long toVertexId)
    {
        _edges.RemoveAll(e => (e.FromVertexId == fromVertexId && e.ToVertexId == toVertexId) || (e.FromVertexId == toVertexId && e.ToVertexId == fromVertexId));
        ResetProperties();
    }

    public Vertex? GetVertexById(long vertexId)
    {
        return _vertices.TryGetValue(vertexId, out var vertex) ? vertex : null;
    }

    private SPointF GetScreenPosition(SPointF graphPosition)
    {
        var screenX =
            Bounds.Center.X +
            (float)((graphPosition.X - CenteredAtGraphPosition.X) * ZoomFactor);

        var screenY =
            Bounds.Center.Y +
            (float)((graphPosition.Y - CenteredAtGraphPosition.Y) * ZoomFactor);

        return new SPointF(screenX, screenY);
    }

    private SPointF GetGraphPosition(SPointF screenPosition)
    {
        var graphX =
            CenteredAtGraphPosition.X +
            (screenPosition.X - Bounds.Center.X) / (float)ZoomFactor;

        var graphY =
            CenteredAtGraphPosition.Y +
            (screenPosition.Y - Bounds.Center.Y) / (float)ZoomFactor;

        return new SPointF(graphX, graphY);
    }

    private double GetEdgeWeight(Edge edge, bool isDynamicWeight, int frameDuration)
    {
        var edgeWeight = edge.Weight;
        if (isDynamicWeight)
        {
            // Gephi calls an overload on Edge.getWeight that takes an interval... but I can't find it
            // in GitHub, so I'm just going to note it here.
            edgeWeight = edge.Weight;
        }

        if (InvertedEdgeWeightsMode)
        {
            return edgeWeight == 0d
                ? 0d
                : 1d / edgeWeight;
        }

        return edgeWeight;
    }

    private int GetDegree(Vertex vertex)
    {
        int degree = 0;
        foreach (var edge in _edges)
        {
            if (edge.FromVertexId == vertex.Id || edge.ToVertexId == vertex.Id)
            {
                degree++;
            }
        }
        return degree;
    }

    private void RunRepulsion(IReadOnlyList<Vertex> vertices,
        bool barnesHutOptimize,
        double barnesHutTheta,
        double gravity,
        RepulsionForce gravityForce,
        double scaling,
        Region? rootRegion,
        RepulsionForce repulsionForce)
    {
        if (barnesHutOptimize)
        {
            if (rootRegion == null)
            {
                throw new InvalidOperationException("Barnes-Hut optimization is enabled, but the root region is null.");
            }

            for (int nIndex = 0; nIndex < vertices.Count; nIndex++)
            {
                var vertex = vertices[nIndex];
                rootRegion.ApplyForce(vertex, repulsionForce, barnesHutTheta);
            }
        }
        else
        {
            for (var v1Index = 0; v1Index < vertices.Count; v1Index++)
            {
                var vertex1 = vertices[v1Index];
                for (var v2Index = 0; v2Index < v1Index; v2Index++)
                {
                    var vertex2 = vertices[v2Index];
                    repulsionForce.Apply(vertex1, vertex2);
                }
            }
        }

        for (int vIndex = 0; vIndex < vertices.Count; vIndex++)
        {
            var vertex = vertices[vIndex];
            gravityForce.Apply(vertex, gravity / scaling);
        }
    }

    public void ResetProperties()
    {
        var vertexCount = _vertices.Count;

        if (vertexCount >= 100)
        {
            ScalingRatio = 15d;
        }
        else
        {
            ScalingRatio = 25d;
        }

        StrongGravityMode = false;
        InvertedEdgeWeightsMode = false;
        Gravity = 0.5d;
        OutboundAttractionDistribution = false;
        LinLogMode = false;
        AdjustSizes = true;
        EdgeWeightInfluence = 1d;
        NormalizeEdgeWeights = false;

        JitterTolerance = 0.1d;
        BarnesHutOptimize = vertexCount >= 1000;
        BarnesHutTheta = 1.2d;
    }
}
