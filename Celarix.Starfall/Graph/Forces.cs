using AngleSharp.Dom;
using Celarix.Starfall.Graph;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Graph;

public abstract class RepulsionForce
{
    public abstract void Apply(Vertex v1, Vertex v2);
    public abstract void Apply(Vertex vertex, Region region);
    public abstract void Apply(Vertex vertex, double g, SPointF center);
}

public sealed class LinearRepulsion : RepulsionForce
{
    private readonly double _coefficient;

    public LinearRepulsion(double coefficient)
    {
        _coefficient = coefficient;
    }

    public override void Apply(Vertex v1, Vertex v2)
    {
        var v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        var v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        var xDistance = v1.Position.X - v2.Position.X;
        var yDistance = v1.Position.Y - v2.Position.Y;
        var distance = Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));

        if (distance > 0)
        {
            var factor = _coefficient * v1Layout.Mass * v2Layout.Mass / (distance * distance);
            v1Layout.DX += xDistance * factor;
            v1Layout.DY += yDistance * factor;

            v2Layout.DX -= xDistance * factor;
            v2Layout.DY -= yDistance * factor;
        }
    }

    public override void Apply(Vertex vertex, Region region)
    {
        var vertexLayout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        var xDistance = vertex.Position.X - region.MassCenterX;
        var yDistance = vertex.Position.Y - region.MassCenterY;
        var distance = Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));

        if (distance > 0)
        {
            var factor = _coefficient * vertexLayout.Mass * region.Mass / (distance * distance);
            vertexLayout.DX += xDistance * factor;
            vertexLayout.DY += yDistance * factor;
        }
    }

    public override void Apply(Vertex vertex, double g, SPointF center)
    {
        var vertexLayout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        var xDistance = vertex.Position.X - center.X;
        var yDistance = vertex.Position.Y - center.Y;
        var distance = Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));

        if (distance > 0)
        {
            var factor = _coefficient * vertexLayout.Mass * g / distance;
            vertexLayout.DX -= xDistance * factor;
            vertexLayout.DY -= yDistance * factor;
        }
    }
}

public class LinearRepulsionAntiCollision : RepulsionForce
{

    private readonly double _coefficient;

    public LinearRepulsionAntiCollision(double c)
    {
        _coefficient = c;
    }


    public override void Apply(Vertex v1, Vertex v2)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        var centerDistance = Math.Sqrt(xDist * xDist + yDist * yDist);

        // An exact overlap has no geometric direction for the force to follow. Give it a stable
        // direction based on the vertex IDs so coincident vertices can separate reproducibly.
        if (centerDistance == 0d)
        {
            xDist = v1.Id < v2.Id ? 1d : -1d;
            centerDistance = 1d;
        }

        double distance = centerDistance - v1.Size - v2.Size;

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = _coefficient * v1Layout.Mass * v2Layout.Mass / distance / distance;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;

        }
        else if (distance < 0)
        {
            double factor = 100 * _coefficient * v1Layout.Mass * v2Layout.Mass;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }


    public override void Apply(Vertex vertex, Region region)
    {
        VertexLayoutData vLayout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = vertex.Position.X - region.MassCenterX;
        double yDist = vertex.Position.Y - region.MassCenterY;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist);

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = _coefficient * vLayout.Mass * region.Mass / distance / distance;

            vLayout.DX += xDist * factor;
            vLayout.DY += yDist * factor;
        }
        else if (distance < 0)
        {
            double factor = -_coefficient * vLayout.Mass * region.Mass / distance;

            vLayout.DX += xDist * factor;
            vLayout.DY += yDist * factor;
        }
    }

    public override void Apply(Vertex vertex, double g, SPointF center)
    {
        VertexLayoutData vLayout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = vertex.Position.X - center.X;
        double yDist = vertex.Position.Y - center.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist);

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = _coefficient * vLayout.Mass * g / distance;

            vLayout.DX -= xDist * factor;
            vLayout.DY -= yDist * factor;
        }
    }
}

public class StrongGravity : RepulsionForce
{

    private readonly double _coefficient;

    public StrongGravity(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2)
    {
        // Not Relevant
    }

    public override void Apply(Vertex vertex, Region region)
    {
        // Not Relevant
    }

    public override void Apply(Vertex vertex, double g, SPointF center)
    {
        VertexLayoutData vLayout = vertex.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = vertex.Position.X - center.X;
        double yDist = vertex.Position.Y - center.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist);

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = _coefficient * vLayout.Mass * g;

            vLayout.DX -= xDist * factor;
            vLayout.DY -= yDist * factor;
        }
    }
}

public abstract class AttractionForce
{
    public abstract void Apply(Vertex v1, Vertex v2, double e);
}

/*
 * Attraction force: Linear
 */
public class LinearAttraction : AttractionForce
{

    private readonly double _coefficient;

    public LinearAttraction(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;

        // NB: factor = force / distance
        double factor = -_coefficient * e;

        v1Layout.DX += xDist * factor;
        v1Layout.DY += yDist * factor;

        v2Layout.DX -= xDist * factor;
        v2Layout.DY -= yDist * factor;
    }
}

public class LinearAttractionAntiCollision : AttractionForce
{
    private readonly double _coefficient;

    public LinearAttractionAntiCollision(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        var v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        var v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        var xDist = v1.Position.X - v2.Position.X;
        var yDist = v1.Position.Y - v2.Position.Y;
        var distance = MathHelpers.Distance(xDist, yDist);

        if (distance > 0)
        {
            var factor = -_coefficient * e;
            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;
            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Linear, distributed by mass (typically, degree)
 */
public class LinearAttractionMassDistributed : AttractionForce
{

    private readonly double _coefficient;

    public LinearAttractionMassDistributed(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;

        // NB: factor = force / distance
        double factor = -_coefficient * e / v1Layout.Mass;

        v1Layout.DX += xDist * factor;
        v1Layout.DY += yDist * factor;

        v2Layout.DX -= xDist * factor;
        v2Layout.DY -= yDist * factor;
    }
}

/*
 * Attraction force: Logarithmic
 */
public class LogAttraction : AttractionForce
{

    private readonly double _coefficient;

    public LogAttraction(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist);

        if (distance > 0)
        {

            // NB: factor = force / distance
            double factor = -_coefficient * e * Math.Log(1 + distance) / distance;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Linear, distributed by Degree
 */
public class LogAttractionDegreeDistributed : AttractionForce
{

    private readonly double _coefficient;

    public LogAttractionDegreeDistributed(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist);

        if (distance > 0)
        {

            // NB: factor = force / distance
            double factor = -_coefficient * e * Math.Log(1 + distance) / distance / v1Layout.Mass;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Linear, with Anti-Collision
 */
public class linAttraction_antiCollision : AttractionForce
{

    private readonly double _coefficient;

    public linAttraction_antiCollision(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist) - v1.Size - v2.Size;

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = -_coefficient * e;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Linear, distributed by Degree, with Anti-Collision
 */
public class LinearAttractionDegreeDistributedAntiCollision : AttractionForce
{

    private readonly double _coefficient;

    public LinearAttractionDegreeDistributedAntiCollision(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist) - v1.Size - v2.Size;

        if (distance > 0)
        {
            // NB: factor = force / distance
            double factor = -_coefficient * e / v1Layout.Mass;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Logarithmic, with Anti-Collision
 */
public class LogAttractionAntiCollision : AttractionForce
{

    private readonly double _coefficient;

    public LogAttractionAntiCollision(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist) - v1.Size - v2.Size;

        if (distance > 0)
        {

            // NB: factor = force / distance
            double factor = -_coefficient * e * Math.Log(1 + distance) / distance;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}

/*
 * Attraction force: Linear, distributed by Degree, with Anti-Collision
 */
public class LogAttractionDegreeDistributedAntiCollision : AttractionForce
{

    private readonly double _coefficient;

    public LogAttractionDegreeDistributedAntiCollision(double c)
    {
        _coefficient = c;
    }

    public override void Apply(Vertex v1, Vertex v2, double e)
    {
        VertexLayoutData v1Layout = v1.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");
        VertexLayoutData v2Layout = v2.LayoutData ?? throw new InvalidOperationException("Vertex layout data is not initialized.");

        // Get the distance
        double xDist = v1.Position.X - v2.Position.X;
        double yDist = v1.Position.Y - v2.Position.Y;
        double distance = Math.Sqrt(xDist * xDist + yDist * yDist) - v1.Size - v2.Size;

        if (distance > 0)
        {

            // NB: factor = force / distance
            double factor = -_coefficient * e * Math.Log(1 + distance) / distance / v1Layout.Mass;

            v1Layout.DX += xDist * factor;
            v1Layout.DY += yDist * factor;

            v2Layout.DX -= xDist * factor;
            v2Layout.DY -= yDist * factor;
        }
    }
}
