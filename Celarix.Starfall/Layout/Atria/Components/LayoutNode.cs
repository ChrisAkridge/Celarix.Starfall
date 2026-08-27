using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Components;

public sealed class LayoutNode
{
    private enum DivisionType
    {
        NotDivided,
        Inset,
        Horizontal,
        Vertical,
    }

    private DivisionType _divisionType;
    private LayoutNode? _firstChild;
    private LayoutNode? _secondChild;

    public SRectF NormalizedBounds { get; }
    public string? Name { get; }

    public LayoutNode()
    {
        NormalizedBounds = new SRectF(0d, 0d, 1d, 1d);
        _divisionType = DivisionType.NotDivided;
    }

    public LayoutNode(SRectF normalizedBounds, string? name = null)
    {
        NormalizedBounds = normalizedBounds;
        Name = name;
        _divisionType = DivisionType.NotDivided;
    }

    public (LayoutNode Left, LayoutNode Right) SplitHorizontal(string leftName, string rightName, double leftPortion)
    {
        if (_divisionType != DivisionType.NotDivided)
        {
            throw new InvalidOperationException("Cannot split a node that has already been split.");
        }

        if (!double.IsFinite(leftPortion) ||
            leftPortion < 0 ||
            leftPortion > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(leftPortion));
        }

        _divisionType = DivisionType.Horizontal;
        var leftBounds = new SRectF(
            NormalizedBounds.X,
            NormalizedBounds.Y,
            NormalizedBounds.Width * leftPortion,
            NormalizedBounds.Height);
        var rightBounds = new SRectF(
            NormalizedBounds.X + leftBounds.Width,
            NormalizedBounds.Y,
            NormalizedBounds.Width - leftBounds.Width,
            NormalizedBounds.Height);
        _firstChild = new LayoutNode(leftBounds, leftName);
        _secondChild = new LayoutNode(rightBounds, rightName);
        return (_firstChild, _secondChild);
    }

    public (LayoutNode Top, LayoutNode Bottom) SplitVertical(string topName, string bottomName, double topPortion)
    {
        if (_divisionType != DivisionType.NotDivided)
        {
            throw new InvalidOperationException("Cannot split a node that has already been split.");
        }

        if (!double.IsFinite(topPortion) ||
            topPortion < 0 ||
            topPortion > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(topPortion));
        }

        _divisionType = DivisionType.Vertical;

        var topBounds = new SRectF(
            NormalizedBounds.X,
            NormalizedBounds.Y,
            NormalizedBounds.Width,
            NormalizedBounds.Height * topPortion);
        var bottomBounds = new SRectF(
            NormalizedBounds.X,
            NormalizedBounds.Y + topBounds.Height,
            NormalizedBounds.Width,
            NormalizedBounds.Height - topBounds.Height);
        _firstChild = new LayoutNode(topBounds, topName);
        _secondChild = new LayoutNode(bottomBounds, bottomName);
        return (_firstChild, _secondChild);
    }

    public LayoutNode Inset(double horizontalFraction, double verticalFraction, string name)
    {
        if (_divisionType != DivisionType.NotDivided)
        {
            throw new InvalidOperationException("Cannot split a node that has already been split.");
        }
        
        if (!double.IsFinite(horizontalFraction) ||
            horizontalFraction < 0 ||
            horizontalFraction > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(horizontalFraction));
        }

        if (!double.IsFinite(verticalFraction) ||
            verticalFraction < 0 ||
            verticalFraction > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(verticalFraction));
        }

        _divisionType = DivisionType.Inset;

        var insetWidth = NormalizedBounds.Width * horizontalFraction;
        var insetHeight = NormalizedBounds.Height * verticalFraction;

        var bounds = new SRectF(
            NormalizedBounds.X + insetWidth,
            NormalizedBounds.Y + insetHeight,
            NormalizedBounds.Width - (2 * insetWidth),
            NormalizedBounds.Height - (2 * insetHeight));

        _firstChild = new LayoutNode(bounds, name);
        return _firstChild;
    }

    public SRectF BoundsFor(string? name, SRectF baseSize)
    {
        if (!TryGetBoundsFor(name, baseSize, out var result))
        {
            throw new ArgumentException($"No node with name '{name}' exists in this layout.");
        }

        return result.Value;
    }

    public bool TryGetBoundsFor(string? name, SRectF baseSize,
        [NotNullWhen(true)] out SRectF? result)
    {
        if (name == null)
        {
            // The caller is asking about the root node, which implicitly is the same size as the base size.
            result = baseSize;
            return true;
        }

        if (name == Name)
        {
            // Hey, it's us! Return our bounds relative to the base size.
            result = new SRectF(
                baseSize.X + NormalizedBounds.X * baseSize.Width,
                baseSize.Y + NormalizedBounds.Y * baseSize.Height,
                NormalizedBounds.Width * baseSize.Width,
                NormalizedBounds.Height * baseSize.Height);
            return true;
        }
        
        if (_firstChild != null)
        {
            if (_firstChild.TryGetBoundsFor(name, baseSize, out result))
            {
                return true;
            }
        }
        
        if (_secondChild != null)
        {
            if (_secondChild.TryGetBoundsFor(name, baseSize, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }
}
