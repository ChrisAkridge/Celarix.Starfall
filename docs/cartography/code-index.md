# Code index

This index points to architectural entry points, not every type.

## Hosts and orchestration

- `Celarix.Starfall.Presentations/Program.cs` — production argument parsing and recovery loop.
- `Celarix.Starfall.Presentations/PresentationRunner.cs` — production composition root, slide registry, monitor selection, and navigation.
- `Celarix.Starfall.Playground/Program.cs` — experiment selector.
- `Celarix.Starfall/Presentation/PresentationEngine.cs` — generic scene graph and transition runner; currently not referenced by active callers.
- `Celarix.Starfall/Presentation/Graph/` — generic scene nodes and directed transition edges.

## Atria

- `Celarix.Starfall/Layout/Atria/AtriaLayoutEngine.cs` — current-slide lifecycle and frame orchestration.
- `Celarix.Starfall/Layout/Atria/AtriaSlide.cs` — base slide lifecycle, elements, navigation, update, and rendering.
- `Celarix.Starfall/Layout/Atria/LayeredAtriaSlide.cs` — explicit layer-order rendering.
- `Celarix.Starfall/Layout/Atria/Elements/AtriaElement.cs` — reusable element base class.
- `Celarix.Starfall/Layout/Atria/Components/LayoutNode.cs` — normalized split/inset layout tree.
- `Celarix.Starfall/Layout/Atria/Components/Grid.cs` and `LayoutStacker.cs` — reusable layout containers.
- `Celarix.Starfall/Layout/Atria/Animation/AnimationContext.cs` — scheduling and owner-scoped lifetime.
- `Celarix.Starfall/Layout/Atria/Animation/AnimationSlot.cs` — animation replacement policy.
- `Celarix.Starfall/Layout/Atria/StateMachine.cs` — attribute-discovered slide transitions; linear helper is incomplete.

## Rendering

- `Celarix.Starfall/Rendering/Targets/IRenderTarget.cs` — backend contract.
- `Celarix.Starfall/Rendering/Targets/SkiaTkTarget.cs` — interactive OpenTK/Skia target.
- `Celarix.Starfall/Rendering/Targets/SkiaPngTarget.cs` — numbered PNG-frame target.
- `Celarix.Starfall/Rendering/Targets/SkiaOffscreenTarget.cs` — image-producing target for composition.
- `Celarix.Starfall/Rendering/Targets/SkiaCommon.cs` — shared Skia primitive implementation.
- `Celarix.Starfall/Rendering/MeasurementService.cs` — cached target-specific text measurement.
- `Celarix.Starfall/Rendering/Models/` — backend-neutral geometry, paint, font, image, input, and path values.

## Libra

- `Celarix.Starfall/Libra/LibraExpressions.cs` — fluent/direct construction helpers.
- `Celarix.Starfall/Libra/Expressions/LibraExpression.cs` — expression base, IDs, replacement, and parse entry.
- `Celarix.Starfall/Libra/Parsing/Lexer.cs` — tokenization.
- `Celarix.Starfall/Libra/Parsing/LibraParser.cs` — Pratt-style expression parsing.
- `Celarix.Starfall/Libra/Parsing/Binding/LibraBinder.cs` — syntax-to-expression binding.
- `Celarix.Starfall/Libra/Metrics/` — expression-specific geometry calculations.
- `Celarix.Starfall/Libra/Renderables/` — backend-neutral final drawing objects.
- `Celarix.Starfall/Libra/LibraLayoutResult.cs` — immutable layout output.

## Charts

- `Celarix.Starfall/Charts/DataResolution/DataResolver.cs` — inclusive X-range bucketing.
- `Celarix.Starfall/Charts/DataResolution/IDataSource.cs` — data boundary.
- `Celarix.Starfall/Charts/DataResolution/IResolutionStrategy.cs` — bucket aggregation boundary.
- `Celarix.Starfall/Charts/DataResolution/StandardResolutionStrategy.cs` — standard empty/individual/aggregate policy.
- `Celarix.Starfall/Charts/Displays/IChartDisplay.cs` — plot-rendering boundary.
- `Celarix.Starfall/Layout/Atria/Elements/ChartElement.cs` — Atria container and chart chrome.
- `Celarix.Starfall/Layout/Atria/Elements/BarChartElement.cs` — older self-contained bar chart.

## Tests and design notes

- `Celarix.Starfall.Tests/Libra/Parsing/` — lexer, parser, and binder coverage.
- `Celarix.Starfall.Tests/Charts/` — chart core and integral-axis label coverage.
- `Celarix.Starfall.Tests/Atria/Components/` — grid and layout-stacker coverage.
- `AI Notes/` — forward-looking design records. Treat them as proposed intent unless behavior is also present in source.

