# Celarix.Starfall codebase map

Celarix.Starfall is a .NET 10, code-first presentation system. Presentations are ordinary C# programs whose slides own layout, animation, and drawing behavior. The active production host builds Atria slides, sends them through a frame-driven layout engine, and renders them with Skia either to an OpenTK window or to PNG frames.

This map intentionally excludes the deprecated Helium and Delphinus paths, including named demos, adapters, and references. Claims here describe the remaining active code only.

## System map

| Area | Responsibility | Architectural role |
| --- | --- | --- |
| `Celarix.Starfall.Presentations` | Builds and runs the July 2026 floating-point presentation | Main production executable and composition root |
| `Celarix.Starfall.Playground` | Selects experimental presentations from a command-line argument | Development host and examples |
| `Layout/Atria` | Owns slides, elements, relative layout, slide-local state, and animation scheduling | Primary presentation model |
| `Rendering` | Defines backend-neutral drawing primitives and implements Skia targets | Boundary between presentation semantics and output devices |
| `Libra` | Builds, parses, lays out, and renders mathematical expressions | Specialized content subsystem used by slides and charts |
| `Charts` | Resolves data at a requested density and renders chart displays/elements | Specialized visualization subsystem |
| `Presentation` | Models scenes and directed transitions generically | Separate, currently unintegrated orchestration layer |
| `Celarix.Starfall.Tests` | Tests Libra parsing/binding, chart logic, and Atria layout components | Automated behavioral evidence |

The dependency direction is broadly:

```text
Presentation hosts
    -> Atria slides/elements
        -> Charts and Libra
        -> Rendering contracts and models
            -> Skia/OpenTK or PNG output
```

The generic `PresentationEngine<TScene,TTransition>` also depends on a generic layout-engine contract, but repository searches show no active caller. The production presentation instead switches named Atria slides directly through `AtriaLayoutEngine`.

## Start here

- [Architecture and boundaries](architecture.md) explains ownership, concepts, extension points, and architectural uncertainties.
- [Runtime flows](runtime-flows.md) follows startup, frame rendering, slide advancement, animation, Libra layout, and chart data resolution.
- [Code index](code-index.md) maps concepts to their most useful source entry points.

## Vocabulary

- **Slide**: an `AtriaSlide` containing elements and optional advance/rewind behavior.
- **Element**: an `AtriaElement` with bounds, transform-like visual properties, lifecycle hooks, and a `Render` implementation.
- **Render target**: an `IRenderTarget` that turns Starfall drawing models into pixels or files.
- **Animation context**: the owner-scoped scheduler for fixed-duration and continuing animations.
- **Animation slot**: a named ownership point that defines what happens when one animation replaces another.
- **Libra expression**: an expression tree that lays itself out into backend-neutral renderables.
- **Resolved data point**: an empty, individual, or aggregated representation of one chart X bucket.

