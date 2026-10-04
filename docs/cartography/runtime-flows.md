# Runtime flows

## Production startup and a rendered frame

1. `Program` parses viewport dimensions and the optional recovery flag.
2. `PresentationRunner` asks the user for a monitor and constructs `AtriaLayoutEngine`.
3. It constructs `SkiaTkTarget`, passing the layout engine as `INotifyFrameRequested`.
4. The runner attaches keyboard and exception handlers, assigns the target, creates a target-backed `MeasurementService`, and registers slide factories.
5. The first factory creates a slide. `AddSlide` injects shared services and calls the slide's `Initialize` method.
6. `SkiaTkTarget.Start` enters the OpenTK loop. Each render callback forwards elapsed time to `AtriaLayoutEngine.OnFrameRequested`.
7. The engine increments the global frame, updates all registered animation contexts, and updates the current slide and elements.
8. The slide clears/draws itself and its elements through `IRenderTarget`; `Complete` flushes Skia and swaps buffers.

The frame callback therefore originates at the output boundary and flows inward to presentation state before drawing flows outward through the same target.

## Advancing and rewinding

Right-arrow input calls `AdvanceCurrentSlide`. A slide may mutate internal state and return `InternalStateChanged`; otherwise it returns `CanAdvance`, and the runner replaces the current slide with the next factory product. Replacement disposes the previous instance, so slide-local animations and resources do not survive the boundary.

Left-arrow input deliberately uses reconstruction. The first press recreates the current slide in its initial state; a subsequent press moves to the previous slide. This avoids requiring reliable reverse transitions from every slide. `R` opens a console slide picker and replaces the slide with the selected factory product.

## An element animation

When an element is attached to a slide, it receives an animation context owned by that element. Code schedules a fixed-duration or continuing animation directly, or places it in an `AnimationSlot`. On every engine frame, the registry updates the context once. Animation callbacks mutate element or model properties; the following render observes those values.

Slots matter when input arrives before an earlier animation completes. The caller selects whether replacement finishes, cancels, or detaches from the existing animation. Disposing the slide cascades to elements and contexts, which force-finish outstanding work.

## Libra text-to-renderables

For parsed expressions:

```text
source string
  -> Lexer tokens
  -> LibraParser syntax tree
  -> validation and LibraBinder
  -> LibraExpression tree
  -> recursive metrics/layout
  -> LibraLayoutResult
  -> LibraRenderable.RenderAt(IRenderTarget)
```

The build context carries colors, opacity, fence behavior, IDs, and substitutions through binding. Layout produces backend-independent positions and geometry. The final render step converts Libra paths to Starfall paths and draws via the active target. Directly constructed expression trees enter at the `LibraExpression tree` stage.

## Chart data resolution and rendering

`DataResolver.Resolve` divides the requested inclusive X range into bounded buckets. For each bucket, `IDataSource.ResolveBucket` gathers or generates observations and applies its resolution strategy. `StandardResolutionStrategy` preserves one point exactly and summarizes multiple points with first/last/min/max/average/count/sum values.

A chart display receives these resolved values and draws inside bounds supplied by its container. `ChartElement` calculates title, chart, and info-panel regions, delegates the core plot to `IChartDisplay`, and owns visibility animations. Property change notifications tell the display that its container changed, establishing the invalidation boundary between container configuration and display caches.

## Offline PNG rendering

The playground can pair Atria with `SkiaPngTarget`. It follows the same update/draw contract, but `Complete` encodes the current bitmap as `frame_########.png`, disposes the frame canvas/bitmap, and allocates a fresh pair. `Start` is a no-op, so the caller is responsible for driving frames.

