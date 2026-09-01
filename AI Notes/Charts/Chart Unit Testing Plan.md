# Chart Unit Testing Plan

## Purpose

Add focused correctness tests alongside Phase 1 and Phase 2 implementation. Testing is not a separate final phase: each behavioral change should arrive with coverage for its public contract and important invariants.

Keep performance measurements out of ordinary unit tests. Unit tests must remain deterministic, fast, and independent of machine speed.

## General Conventions

- Use small hand-verifiable examples for exact behavior.
- Use deterministic generated sequences for broader numerical cases.
- Compare floating-point results with tolerances appropriate to the operation and magnitude.
- Prefer state and event-count assertions over implementation-detail assertions.
- Verify both the successful path and rollback/failure paths.
- Do not assert wall-clock deadlines in unit tests.
- Use enormous `BigInteger` gaps in small tests to prove that algorithms scale with stored points rather than numeric range width.

## Phase 1 Tests

### `UpdatePropertiesAtomic`

- A successful multi-property update raises exactly one `PropertiesChanged` event.
- Assigning only values equal to their existing values raises no event.
- Final validation observes the complete proposed state rather than intermediate assignments.
- A final validation failure restores every changed field.
- An exception thrown by the callback restores every changed field.
- Rollback raises no property-change event.
- Several assignments to the same property roll back to the original value.
- Nested or reentrant calls throw `InvalidOperationException` and leave the object unchanged.
- After success or failure, later ordinary setters and atomic updates still work.

### Whole-Collection Percentile Replacement

- Input is copied rather than retained by reference.
- The installed collection is sorted according to the public contract.
- Empty-list behavior agrees with percentile visibility rules.
- Out-of-range values are rejected.
- Duplicate values are rejected.
- Mutating the caller's original collection after the update has no effect.
- A rejected replacement preserves the previous collection and raises no event.
- Percentile replacement participates correctly in `UpdatePropertiesAtomic` rollback.

### Manual Connections

- Construction performs the initial connection exactly once.
- A connected dependency change invalidates or updates its consumer once.
- `Disconnect` is idempotent.
- Changes after disconnection do not notify the consumer.
- If reconnection is supported, reconnecting restores notification without duplicate subscriptions.
- Injected dependencies are not disposed or mutated by disconnection.
- A disconnected display follows its documented rendering behavior.

### Visibility Animation in `ChartElement`

- `ChartElement` owns separate title and info-panel animation slots.
- A visibility request enters the correct appearing/disappearing state.
- Completion produces the correct stable state and clears transitional progress.
- Duplicate requests toward the current target do not restart the animation.
- Reversing an active animation begins at the current visual progress without a jump.
- Detachment does not force completion, cancellation, or state normalization.

### Non-Finite Normalization and Warnings

- NaN becomes zero.
- Positive infinity becomes zero.
- Negative infinity becomes zero.
- Stored values and all statistical indexes observe the normalized zero.
- A source or series warns on its first non-finite value.
- Further non-finite values from the same instance do not emit additional warnings.
- Separate source/series instances each receive their own first warning.

### `DataSeriesPoint` Equality

- Same X and different Y values are equal by default.
- Equal default values have equal hash codes.
- Different X values are unequal by default.
- The exact comparer includes both X and Y.
- The exact comparer handles nullable Y consistently.
- `HashSet<DataSeriesPoint>` and `Dictionary<DataSeriesPoint, ...>` obey the X-only default identity.

## Phase 2 Tests

### General Data Sources and Events

- `BarChartDisplay` can consume an injected non-`DataSeries` source.
- The ordinary `DataSeries` convenience path still constructs or adapts the intended source.
- Displays subscribe to `IDataSource.DataChanged` uniformly.
- A mutable source event invalidates the display once.
- An immutable source may never raise the event and requires no special consumer logic.
- Disconnecting the display unregisters its source event handler.

### Welford Addition

- Empty-series behavior remains defined.
- One value produces its own mean and zero population variance.
- Known small examples produce the expected population and sample variance.
- Repeated identical values produce zero variance.
- Large-offset, small-spread values retain a meaningful variance.
- Count, sum, mean, `M2`, extrema, and sorted-frequency state remain mutually consistent.

### Welford Removal and Replacement

- Removing either value from a two-value series leaves one value with zero `M2`.
- Removing the final numeric value resets scalar statistical state.
- Add followed by remove returns to the original state within tolerance.
- Numeric-to-numeric replacement updates statistics and raises one event.
- Numeric-to-null replacement removes one statistical observation.
- Null-to-numeric replacement adds one statistical observation.
- Null-to-null replacement follows the selected identical-assignment event contract.
- A negligible negative `M2` is clamped to zero.
- A materially invalid `M2` invokes the full-recalculation recovery path.

### Explicit Statistics Recalculation

- Recalculation agrees with incremental state after ordinary mutations.
- Recalculation agrees after a long deterministic mutation sequence.
- Recalculation rebuilds sorted frequencies and extrema as well as Welford state.
- Recalculation does not raise `DataChanged`.
- Construction and explicit recalculation use behaviorally equivalent rebuild logic.

### Ordered Stored-Series Range Lookup

- Results are ordered by X.
- Inclusive minimum and maximum behavior is correct.
- Null/missing observations follow the documented result contract.
- Empty ranges of stored observations return quickly and correctly.
- Points outside the range are excluded.
- A few points separated by `BigInteger` distances such as `10^1000` are queried without iterating the intervening integers.
- Adding, removing, and replacing points keeps the ordered index consistent.

### Bucket Creation and Exact Aggregation

- Bucket ranges are ordered, contiguous, non-overlapping, and cover the requested inclusive range.
- Requested bucket counts larger than cardinality do not create invalid or empty-width ranges.
- Every stored point belongs to at most one bucket.
- Exact stored aggregation preserves count, sum, minimum, maximum, first, last, and stable average.
- Empty and individual buckets return their distinct resolved types.
- Function evaluation count remains bounded by bucket count times samples per bucket.

### Bucket-to-Display Mapping

- A sparse individual bucket may occupy multiple display pixels.
- A dense aggregate bucket maps to its intended adjacent display region.
- Shared bucket edges create no gaps or overlaps.
- Fractional viewport limits clip boundary bars correctly.
- Non-integral chart bounds follow the selected snapping policy.
- Dense quantization preserves adjacency after rounding.
- Logical mapping remains independent of source implementation.

### Degenerate Bounds

- Zero-width, sub-pixel-width, and otherwise non-renderable chart regions do not request zero buckets.
- Degenerate bounds render an empty/no-data result without throwing.
- `DataResolver` still rejects an explicitly invalid zero bucket count from ordinary callers.

## Numerical Reference Strategy

For Welford tests, compare against either:

- Hand-calculated exact examples.
- A straightforward two-pass reference calculation for moderate values.
- Decimal or higher-precision reference arithmetic for large-offset cases when useful.

Avoid using the existing sum-of-squares formula as the only oracle, because its instability is the behavior being replaced.

## Test Data Sizes

Ordinary unit tests should generally use tens or hundreds of observations. A few deterministic tests may use thousands if execution remains negligible. The 10,000-to-1,000,000-point scenarios belong in the performance suite, not the correctness suite.

