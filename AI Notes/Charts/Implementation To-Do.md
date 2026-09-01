# Charts Implementation To-Do

This document tracks chart work that has been discussed and explicitly scheduled for implementation. Design ideas that are still under consideration belong in their own notes and should not be added here until accepted.

## Phase 1 — Object and Property Semantics

Implement Phase 1 with the corresponding correctness coverage in `Chart Unit Testing Plan.md`. Tests are part of each change, not a separate cleanup phase.

### Atomic Property Updates

- Rename `SetProperties` to `UpdatePropertiesAtomic`.
- Treat nested or reentrant atomic updates as an error rather than defining nested transaction semantics.
- Suppress validation and property-change events while the update callback is running.
- Record field assignments in an undo log through `SetProperty`.
- Validate the complete object once after the callback succeeds.
- On success, discard the undo log and raise at most one change event when values actually changed.
- If the callback throws or final validation fails, restore assignments in reverse order and raise no event.
- Replace mutable collection-property APIs with whole-collection replacement where practical. In particular, percentile updates should accept a complete collection, copy and validate it, and install it as one property value.

### Manual Connection Ownership

- Replace lambda event subscriptions with named handler methods.
- Add explicit, idempotent connection-management methods to event-subscribing chart types.
- Constructors should perform the initial wiring.
- Disconnecting must unregister handlers without disposing or otherwise taking lifetime ownership of injected dependencies.
- Decide per type whether reconnection is supported and what a disconnected object renders.

### Move Visibility Animation into `ChartElement`

- Move title-visibility and info-panel-visibility animation slots out of `ChartProperties` and into `ChartElement`.
- Keep visibility state available through `ChartProperties`, but restrict mutation of transitional state to the owning chart-element implementation.
- Preserve reversal of an in-progress animation from its current visual progress.
- Follow normal Atria behavior when an element is detached: do not automatically cancel, finish, or normalize its animation.

### Non-Finite Value Warnings

- Normalize NaN and positive/negative infinity to zero at the appropriate series or source ingestion boundary.
- Emit at most one console warning per series or data-source instance.
- Include the first offending X location and value when practical.
- Suppress repeated warnings; optionally retain a count for future diagnostics.

### `DataSeriesPoint` Equality Comparers

- Preserve built-in point identity by X location.
- Ensure built-in `GetHashCode` uses the same X-only semantics as `Equals`.
- Add an explicit comparer for exact X-and-Y equality.
- Consider a Y-only comparer only if an actual grouping/statistical use case appears.

## Phase 2 — Statistics and Practical Data Resolution

Implement Phase 2 with the corresponding correctness coverage in `Chart Unit Testing Plan.md`. After correctness is established, use `Chart Performance and Bulk Data Plan.md` to measure scaling before scheduling optimizations.

### General Data Sources for Displays

- Change `BarChartDisplay` so it can consume an injected `IDataSource` rather than always constructing a `DataSeriesDataSource` internally.
- Preserve a convenient construction path for ordinary `DataSeries` use if it remains useful to presentation authors.
- Give every `IDataSource` a uniform `DataChanged` event. Mutable sources raise it when their resolved output may have changed; immutable/generated sources expose the event but never raise it.
- Let displays subscribe uniformly without capability checks or source-type branching.
- Keep source lifetime ownership manual; injection wires the relationship but does not transfer ownership.

### Welford Statistics

- Replace sum-of-squares variance calculation with incremental Welford mean and `M2` state.
- Support incremental addition, inverse removal, and externally atomic replacement.
- Preserve count, sum, extrema, mode, median, and percentile behavior.
- Clamp only negligible negative `M2` caused by rounding; recalculate on a materially violated invariant.
- Follow the detailed plan in `Welford Statistics Implementation.md`.

### Explicit Statistics Recalculation

- Add `RecalculateStatistics()` backed by the same private full-rebuild path used during construction and invariant recovery.
- Rebuild all statistical indexes and accumulators from normalized stored values.
- Do not raise `DataChanged`, because the underlying observations have not changed.
- Leave global interactive-versus-offline accuracy policy for a later design; the explicit operation provides the necessary hook.

### Replace Integer-Width Range Scanning

- Remove the loop over every integer from `XRange.Minimum` through `XRange.Maximum` in stored-series range lookup.
- Query stored observations by ordered X location so cost is related to stored/matching points rather than numeric range width.
- Preserve missing/null observations and deterministic X ordering.
- Select the ordered index only after comparing mutation and query tradeoffs for expected data sets.

### Validate Existing Dense Aggregation

- Preserve the current design in which the display requests a bounded number of buckets, normally near one bucket per horizontal pixel, and stored observations in each bucket are aggregated exactly.
- Review and correct bucket-to-pixel mapping, ordering assumptions, and sampling semantics identified during the current-code review before treating the path as complete.
- Define a shared bucket-to-display mapping that handles buckets spanning multiple pixels, pixel-scale dense buckets, partially visible boundary bars, and non-integral display bounds without requiring the generic data source to understand screen pixels.
- Keep generated-function sampling explicitly distinct from exact stored-series aggregation.

### Stable Bucket Means

- Replace average calculation based solely on naive bucket summation with a numerically stable online mean, using Welford accumulation where appropriate.
- Preserve `SumY` separately because it is part of the aggregate result, while avoiding dependence on a potentially overflowing or cancellation-prone sum for `AverageY`.
- Apply the same non-finite normalization policy before bucket aggregation.

### Degenerate Display Bounds

- Detect chart regions narrower than one drawable pixel or otherwise unable to request a positive bucket count.
- Render an empty/no-data result without calling `DataResolver` with zero buckets.
- Keep `DataResolver`'s positive-bucket-count validation for genuine caller errors.

### Performance Baseline

- Instrument source resolution, label fitting, renderable construction, and drawing separately.
- Exercise realistic calorie and NOAA-like data sets, including views containing approximately 100,000 to 1,000,000 observations.
- Measure both interactive animation and offline/video-quality rendering.
- Add a repeatable BenchmarkDotNet benchmark project or suite using deterministic synthetic data at multiple scales.
- Establish correctness and baseline costs before introducing caches, hierarchical summaries, or reduced-quality interactive modes.

## Not Yet Scheduled

- Invalidation levels and dirty-state propagation.
- Sparse versus generated data-resolution redesign.
- Resolution caches across viewport changes.
- Hierarchical/pre-aggregated range summaries.
- Separate interactive and offline-quality resolution policies.
