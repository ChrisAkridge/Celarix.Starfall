# Render Coordinate Spaces

Some elements define their own internal coordinate systems. `FloatingPointWindowElement` has row coordinates where `CenteredX` maps the row to the element bounds.

This pattern is powerful, but the conversion math can obscure the visual idea.

Layered slides may solve many of these cases at a better level. Instead of making `FloatingPointWindowElement` simulate zoom by changing bit size, it could draw itself at full scale in its own layer. The layer can then pan and zoom through a render-target transform.

Possible render target support:

```csharp
target.PushTransform(transform);
layer.Render(target);
target.PopTransform();
```

or:

```csharp
using (target.Transformed(transform))
{
    layer.Render(target);
}
```

For Skia targets, this likely maps to canvas save/restore plus translate/scale/matrix operations. Other render targets would implement the same semantic transform API.

The public Starfall API probably should not be matrix-first. A friendlier `STransform2D` can expose common operations such as translation, scale, rotation, shear, and origin/pivot, then convert to a matrix internally for composition and render-target execution.

Possible shape:

```csharp
public readonly record struct STransform2D
{
    public SPointF Translation { get; init; }
    public SPointF Scale { get; init; }
    public SAngle Rotation { get; init; }
    public SPointF Shear { get; init; }
    public SPointF Origin { get; init; }

    public SMatrix3x3 ToMatrix();
}
```

Push/pop still feels like the right render-target primitive, even if the pushed value is an `STransform2D` rather than a raw matrix:

```csharp
target.PushTransform(layer.Transform);
```

Internally, the render target can convert the transform to the matrix representation it needs.

With this model, coordinate spaces exist at two levels:

- Layer-level transforms for camera-like pan/zoom of a whole group of elements.
- Element-local coordinate helpers for cases where the element genuinely has an internal model, such as a number line, graph, or timeline.

Possible shape:

```csharp
var row = CoordinateSpace.Centered(Bounds, centeredX);

var screenX = row.ToScreenX(rowX);
var visible = row.SpanVisible(left, right);
```

The intent is not to hide geometry, but to name recurring coordinate transformations.

This note may partially merge with layered slides. Layers provide the main viewport/camera abstraction; coordinate-space helpers remain useful for internal element math.

Current direction:

- Centered row-space helpers may not be needed as a major abstraction once layers exist.
- `STransform2D` should solve the friendly transform authoring problem.
- Geometry helpers should live in a general geometry layer, not only in Atria.
- Render targets should expose push/pop transform behavior.
- Layer transforms are render-only. Anchoring and basis calculations are not affected by layer transforms.

Questions to explore:

- Should render targets also expose a disposable transform scope for ergonomics?
- Should `STransform2D` include shear immediately, or only once a real use appears?
- What geometry primitives should be introduced with the transform work: matrix, polygon, parallelogram, transformed rect?
