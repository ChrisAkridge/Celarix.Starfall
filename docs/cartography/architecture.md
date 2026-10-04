# Architecture and boundaries

## Composition and external boundaries

The solution contains one reusable library, two executable hosts, and one test project. `Celarix.Starfall.Presentations` is Windows-specific because it uses WinForms/OpenTK integration; the library, playground, and tests target platform-neutral .NET 10.

Important external boundaries are:

- OpenTK supplies the interactive window, monitor selection, keyboard events, and render loop.
- SkiaSharp and HarfBuzz supply drawing and shaped-text measurement.
- ImageSharp supports image-oriented presentation code.
- NAudio is isolated behind the small audio area.
- `IRenderTarget` is the internal seam that keeps most slide and element code independent of those libraries.

## Ownership and responsibilities

### Presentation hosts own composition

The production `PresentationRunner` owns viewport configuration, monitor selection, the ordered list of slide factories, keyboard navigation, and last-chance recovery. It constructs the layout engine, render target, and measurement service explicitly; there is no dependency-injection container.

This host-level slide list is currently the effective presentation graph. Right-arrow input first advances the current slide's internal state and only creates the next slide when the current slide reports that it can advance. Left-arrow behavior reconstructs the current or previous slide rather than relying on every slide to implement full reversal.

### Atria owns slide-local behavior

`AtriaLayoutEngine` owns the named slide collection, current-slide selection, the render target, and the global animation registry. Adding a slide injects its measurement/debug/animation services and calls `Initialize`; removing it disposes the slide.

`AtriaSlide` owns its elements and basis elements. Its default update and render methods walk the element list, while subclasses define initialization and may override navigation or rendering. `LayeredAtriaSlide` adds explicit ordered layers for cases where drawing order needs to be separated from the base element collection.

`AtriaElement` is the normal extension point for reusable visual behavior. Elements own their visual state and an owner-scoped animation context after attachment to a slide. Containers such as grids and layout stackers compute bounds; `LayoutNode` provides a lightweight tree of normalized horizontal/vertical splits and insets.

### Animation ownership follows object lifetime

`AnimationContextRegistry` creates contexts for owners and updates every live context once per global frame. A context owns fixed-duration animations, predicate-driven continuing animations, and replacement slots. Disposing a context force-finishes its work and unregisters it.

`AnimationSlot` makes interruption policy explicit: replacement may finish the old animation, cancel it, or leave it running. This is the key abstraction for rapidly toggled state such as chart-panel visibility.

### Rendering owns pixels, not layout semantics

`IRenderTarget` exposes Starfall-native geometry, color, font, image, path, transform, and text-measurement operations. `SkiaCommon` and the Skia text helpers contain shared backend implementation. The interactive target presents an OpenTK-backed Skia surface; the PNG target emits a numbered file for every completed frame; the offscreen target returns an `SImage` for composition.

`MeasurementService` wraps target-specific text measurement and caches results. Atria injects the service into slides so layout code can measure without reaching into a concrete backend.

### Libra owns mathematical expression structure

Libra can be constructed directly through expression classes and `LibraExpressions`, or from its compact string syntax. The string path is lexer -> Pratt-style parser -> validator/binder -> `LibraExpression` tree. Operators, reserved calls, properties, substitutions, and identifiers are registered or bound at dedicated seams.

Each expression recursively computes a `LibraLayoutResult`: immutable positioned renderables, total bounds, baseline, and math axis. Renderables then draw through `IRenderTarget`. IDs support tree selection and replacement, which allows presentation code to transform meaningful parts of an expression rather than raw coordinates.

### Charts separate data density from display

The newer chart path separates `IDataSource`, bucket resolution, `ResolvedDataPoint`, display strategy, and Atria containment. `DataResolver` divides an inclusive integer X range into at most the requested number of buckets. A data source observes a bucket and delegates to an `IResolutionStrategy`; the standard strategy produces empty, single, or statistical aggregate points.

`IChartDisplay` owns drawing within supplied bounds, while `ChartElement` owns title/info-panel chrome, visibility animation, and integration with an Atria slide. `BarChartDisplay` and `DistributionWrapperDisplay` are display implementations. The older `BarChartElement` is a self-contained implementation and therefore represents a parallel chart design.

## Extension model

Normal extension points are:

1. Add a presentation slide by deriving from `AtriaSlide`, composing elements in `Initialize`, and registering a factory in the relevant host.
2. Add reusable visual content by deriving from `AtriaElement`; keep measurement/layout in Starfall models and draw only through `IRenderTarget`.
3. Add a rendering backend by implementing `IRenderTarget` and preserving `Start`/frame/`Complete` semantics as well as text measurement.
4. Add mathematical structure with a `LibraExpression` plus metrics/renderables; add syntax only when the lexer/parser/binder pipeline also needs to expose it.
5. Add chart input by implementing `IDataSource`, aggregation with `IResolutionStrategy`, or presentation with `IChartDisplay`.

## Invariants and assumptions

- An Atria engine must receive a render target before `Start` or `Render`.
- Its `MeasurementService` must be set before a slide is added.
- Slide removal is the lifecycle boundary: removal disposes the slide, its elements, and their animation contexts.
- Animation contexts must update no more than once for a given global frame; the registry/context pair enforces this.
- Fixed-duration timing currently assumes 60 frames per second when converting seconds to frames.
- Libra IDs must be unique within an expression tree when uniqueness validation is applied.
- Render target transforms are stack-based; every pushed transform must eventually be popped.
- Chart resolution treats X ranges as inclusive and never creates more buckets than the range cardinality.

## Architectural uncertainties and transitional areas

These are evidence-backed observations; inferred intent is labeled.

- **Two orchestration models:** the generic graph-based `PresentationEngine` exists, but no active source references it. The production host directly operates `AtriaLayoutEngine`. It is unclear whether the generic engine is future infrastructure or superseded code.
- **Slide transitions are unresolved:** Atria contains an explicit comment describing the lack of a satisfactory cross-slide transition model. Current production navigation uses an instant replacement.
- **Two chart generations coexist:** the display/data-resolution architecture and the monolithic `BarChartElement` overlap. The likely direction toward the separated model is an inference from its clearer boundaries and newer support notes, not an established repository statement.
- **Chart work is incomplete:** `ChartElement` has an empty info-panel-content method, and several design notes describe pending invalidation, data, performance, and test work.
- **Linear state-machine support is partial:** `LinearStateMachine.Advance` has no implementation.
- **Error display is partial:** generic presentation-engine branches contain TODOs for display-level errors.
- **Target mutability differs:** the interactive target maps `IsAnimating` to event-driven window behavior, while the PNG target only stores the value. Callers should not assume identical scheduling behavior across targets.

