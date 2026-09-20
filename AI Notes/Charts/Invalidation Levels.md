# Chart Invalidation Levels

## Status

Design note only. This work is not yet scheduled for implementation.

The current full-invalidation path is already capable of running at 60 FPS on the development machine. Any additional invalidation mechanism should therefore be justified with measurements or with a source whose resolution cost is predictably high. The goal is to make expensive work avoidable without creating a general-purpose dependency framework.

## Why Consider Levels

A full chart rebuild can combine several different kinds of work:

1. Query or resolve source data into visible buckets.
2. Measure chart geometry and axis regions.
3. Measure and fit text labels.
4. Build bar and gridline renderables.
5. Paint the prepared result.

Many changes affect only a suffix of this sequence. A gridline-color change should not need to reevaluate a generated function. A data or viewport change usually does.

The likely benefit is not raw frame rate for today's local examples. It is predictable cost when data sources become analytical, very large, cached, or otherwise expensive.

## Candidate Levels

Use a small hierarchy rather than independent arbitrary flags:

```text
Data resolution
    -> Layout
        -> Renderables
            -> Paint
```

Invalidating an earlier level implies every level below it.

### Data Resolution

The visible resolved data may be wrong.

Examples:

- The data series changed.
- The visible X range changed.
- The bucket count or resolution strategy changed.
- A source-specific sampling setting changed.

### Layout

Resolved values remain valid, but their placement or text layout may be wrong.

Examples:

- Chart display bounds changed.
- X or Y scale changed without requiring a new source query.
- Axis size, font, angle, or margin changed.
- The info panel changed the bounds available to the display.

Whether a viewport change belongs here or at data resolution depends on whether it changes the requested data buckets.

### Renderables

Data and placement remain valid, but cached drawing primitives must be rebuilt.

Examples:

- A bar color formatter changed.
- Gridline style or thickness changed.
- A paint-affecting property is baked into cached renderables.

### Paint

Existing prepared renderables can simply be drawn again.

Examples:

- Opacity changed when opacity is applied at draw time.
- A color changed when color is not baked into cached geometry.
- The containing slide needs another frame without any chart-state change.

## How It Could Work in Practice

The smallest implementation could be a single `ChartInvalidationLevel` field on each display:

```text
None < Paint < Renderables < Layout < DataResolution
```

When a change occurs, the display retains the more expensive of its current pending level and the new requested level. On the next render, it performs the required pipeline stages and then resets the field to `None`.

This ordered-severity model may be simpler than a flags enum because the dependencies are hierarchical. It prevents contradictory states such as requesting data resolution without layout.

Property change events could eventually carry the required level. Initially, named event handlers inside a display could select the level without changing the shared event contract.

Example flow:

```text
DataSeries.DataChanged
    -> BarChartDisplay marks DataResolution
    -> next Render resolves data, lays out, rebuilds renderables, and paints

GridlineColor changed
    -> BarChartDisplay marks Renderables or Paint
    -> next Render skips source resolution and label fitting
```

## Avoiding Over-Abstraction

- Keep invalidation state local to each display.
- Do not construct a general dependency graph.
- Do not require every property in the codebase to declare invalidation metadata.
- Start with named handlers selecting a level explicitly.
- Add event arguments or property metadata only if repeated boilerplate demonstrates the need.
- Preserve a `Full`/data-resolution invalidation escape hatch for correctness.
- Prefer a redundant rebuild over a complicated dependency rule when the cost is small.

## Measurement Plan

Before implementation, instrument the existing full invalidation pipeline and record time spent in:

- Source/bucket resolution.
- Axis-label measurement and fitting.
- Geometry/renderable construction.
- Actual drawing.

Exercise at least:

- A small stored data series.
- A dense stored series.
- A generated function over a wide range.
- Scrolling and zooming.
- Changes that should theoretically require paint only.

If full invalidation remains comfortably below the frame budget and source evaluation is cheap, defer the feature. Revisit it when a concrete source or visual creates pressure.

## Open Questions

1. Is opacity applied at paint time consistently enough to qualify as paint-only?
2. Which viewport changes can reuse already-resolved buckets?
3. Are label layout and chart geometry worth separating, or should both remain one layout level?
4. Does changing a color require rebuilding Libra renderables?
5. Should property bags know invalidation levels, or should their paired displays interpret property changes?
6. How should a wrapper combine its own invalidation with invalidation from the wrapped display?
7. Should instrumentation ship as optional diagnostics or remain development-only?

