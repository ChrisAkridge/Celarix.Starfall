# Layered Atria Slides

An Atria slide could be modeled as a stack of semantic layers. Each layer owns its own elements, and layers with higher enum values render above layers with lower enum values.

Possible shape:

```csharp
public enum FloatingPointLayers
{
    Background,
    BitWindow,
    Labels,
    Overlay
}

public abstract class LayeredAtriaSlide<TLayer> : AtriaSlide
    where TLayer : struct, Enum
{
    protected AtriaLayer this[TLayer layer] { get; }
}
```

Using an enum gives layers semantic names while preserving draw order. The intent is not to hand-author numeric constants; it is to let normal C# enum ordering and recompilation handle renumbering when a new layer is inserted between existing layers.

Possible usage:

```csharp
Layers[FloatingPointLayers.BitWindow].Add(windowElement);
Layers[FloatingPointLayers.Labels].Add(exponentLabel);
```

Layered behavior should be available through `LayeredAtriaSlide<TLayer>`. Simple `AtriaSlide`s can remain flat.

The larger idea is that a layer can have its own transform:

```csharp
layer.Transform = STransform2D.Identity
    .Translated(...)
    .Scaled(2d);
```

Then an element like `FloatingPointWindowElement` would not need to simulate zoom by changing bit font size. It could live on a zoomable/pannable layer, while labels, overlays, or UI elements stay in screen space.

Every layer should support pan/zoom/transform behavior, but using it should be optional. The default transform should be identity:

```text
translation = (0, 0)
scale = 1x
rotation = 0
shear = 0
```

Render order:

- Sort layers by enum order/value.
- Apply the layer transform.
- Render the layer's elements.
- Restore the target transform before the next layer.

Layout and anchors are the hard part. A layer transform raises questions about coordinate spaces:

- Are anchors defined in slide space, layer-local space, or transformed screen space?
- Can a `BasisPoint` live inside a layer?
- Can an element in one layer anchor to a basis point in another layer?
- Does hit testing or querying need to include layer identity?

This overlaps with render coordinate spaces, but layers are a higher-level presentation concept: semantic grouping, draw order, and camera-like transforms.

Current direction:

- Layers are only available through `LayeredAtriaSlide<TLayer>`.
- Every layer supports transforms, but defaults to identity.
- Layer transforms are render-only. Measurement, layout, anchors, and basis calculations are not affected.
- Explicit enum numeric values are allowed. Presentation authors can choose that tradeoff if they want it.
- If `Add(...)` is called on a layered slide without specifying a layer, default to the highest-valued/frontmost layer and print a warning that the frontmost layer was chosen because none was specified.

Questions to explore:

- Should the fallback `Add(...)` warning be console-only, debug-only, or always visible during development?
- Can an element in one layer anchor to a basis point in another layer?
- Should query results include layer identity?
