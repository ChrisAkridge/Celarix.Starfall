# Viewport-Aware Elements

Layer transforms work well for ordinary pan and zoom, but truly enormous zoomable spaces need a different model. An element like a map, large canvas, timeline, graph, or tiled surface should not render its entire world and let the target scale it. It should render only the visible region.

The layer can provide camera behavior, but the element needs to know enough about the current render context to make culling and level-of-detail decisions.

Possible shape:

```csharp
public readonly record struct RenderViewport(
    SRectF ScreenBounds,
    SPolygon LocalVisibleShape,
    STransform2D Transform,
    double EffectiveScale);
```

Then rendering could pass viewport information to every element:

```csharp
public abstract void Render(IRenderTarget target, RenderViewport viewport);
```

or perhaps pass a broader render context:

```csharp
public readonly record struct AtriaRenderContext(
    RenderViewport Viewport,
    STransform2D Transform);

public abstract void Render(IRenderTarget target, AtriaRenderContext context);
```

With rotations and shears, an axis-aligned viewport rectangle is not enough. In element-local coordinates, the visible slide area may be a rotated/sheared parallelogram or general convex polygon. Rather than making every element care about that shape directly, the context can expose helpers:

```csharp
context.Intersects(localBounds);
context.Intersects(localPolygon);
context.GetEffectiveScale();
```

The element can then ask practical questions:

```csharp
if (!context.Intersects(tile.Bounds))
{
    return;
}

var image = context.GetEffectiveScale() < 0.25d
    ? overviewImage
    : detailedImage;
```

Passing viewport/transform context to all elements may be useful even for simple elements. Most can ignore it, but viewport-aware elements can use it to decide what to draw, where, and at what detail level.

For a layered slide, the layer would compute the viewport by applying the inverse layer transform:

```text
screen viewport -> inverse layer transform -> element/local visible shape
```

Potential uses:

- Cull shapes, tiles, labels, or points outside the visible area.
- Select a level of detail based on zoom.
- Draw map-style tiles instead of one enormous surface.
- Render simplified visuals when zoomed out and detailed visuals when zoomed in.
- Avoid precision and performance problems from huge coordinate spaces.

Possible tile/LOD sketch:

```csharp
var visibleTiles = TileGrid.GetVisibleTiles(context.Viewport.LocalVisibleShape, context.Viewport.EffectiveScale);

foreach (var tile in visibleTiles)
{
    DrawTile(target, tile);
}
```

This complements layered slides:

- Layers provide semantic ordering and camera transforms.
- Viewport-aware elements provide large-world rendering intelligence.
- Render targets provide transform and clipping primitives.

Current direction:

- `AtriaElement.Render` should receive render context.
- The context should include transform/viewport information, not just an axis-aligned rectangle.
- The context should provide helper methods so elements can ask "does this thing intersect the visible area?" without understanding every viewport shape.
- The context should expose effective scale or enough transform information for level-of-detail decisions.

Questions to explore:

- Should the public render parameter be `RenderViewport`, `AtriaRenderContext`, or another type?
- Should `EffectiveScale` be one value, or should non-uniform scale expose X/Y scale separately?
- Should render targets expose clipping scopes alongside transform scopes?
- Should viewport/context include slide space, layer space, element-local space, or helper methods to convert between them?
- How should this interact with anchors and basis elements?
