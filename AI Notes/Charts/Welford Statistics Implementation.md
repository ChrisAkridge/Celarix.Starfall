# Welford Statistics Implementation

## Status

Design plan only. Welford variance accumulation is not yet scheduled for implementation.

## Goals

- Replace variance calculations based on `sumOfSquares - (sum * sum / count)` with Welford's numerically stable online algorithm.
- Preserve efficient incremental addition, removal, and replacement of data-series points.
- Preserve existing count, sum, minimum, maximum, percentile, median, and mode behavior.
- Continue allowing mathematically unusual data sets and domains without imposing a semantic interpretation on their statistics.
- Normalize non-finite observations consistently before they reach any stored or accumulated state.
- Provide an explicit full-statistics recalculation operation for maintenance, verification, or high-accuracy rendering modes.

## Series State

`DataSeries` should maintain at least:

```text
count
sum
mean
M2
sorted value frequencies
minimum
maximum
```

`M2` is the sum of squared distances from the running mean. It replaces `sumOfSquares` as the basis for variance.

Derived values are:

```text
population variance = M2 / count
sample variance     = M2 / (count - 1)
standard deviation = sqrt(variance)
```

Existing empty-series and insufficient-sample behavior should remain explicit.

## Non-Finite Normalization

Normalize a Y value before storing it or applying any statistical mutation:

```text
NaN                 -> 0
positive infinity   -> 0
negative infinity   -> 0
```

The normalized value must be the stored value. This ensures that later removal reverses the same contribution that addition made and that sorting, extrema, percentiles, sum, and variance all observe identical data.

Each series or producing data-source instance should emit at most one console warning. The first warning should include the source identity, X location, and original value when practical. Later occurrences should be suppressed; an internal suppressed-warning count may be retained for future diagnostics.

## Addition

For a new normalized value `x`:

```text
newCount = count + 1
delta    = x - mean
newMean  = mean + (delta / newCount)
delta2   = x - newMean
newM2    = M2 + (delta * delta2)
```

For the first value, initialize count to one, mean to the value, and `M2` to zero.

Update sum and the sorted-value frequency index alongside the Welford state. Update minimum and maximum using the existing incremental approach.

## Removal

When removing a normalized value `x` from a series with more than one value:

```text
newCount = count - 1
newMean  = ((count * mean) - x) / newCount
newM2    = M2 - ((x - mean) * (x - newMean))
```

Special cases:

- Removing the only value resets all scalar statistical state.
- Removing from a two-value series leaves one value with `M2` equal to zero.
- Update the sorted-value frequency index before deriving replacement minimum and maximum values.
- Subtract the normalized stored value from sum.

Floating-point rounding may produce a very small negative `M2`. Clamp values within a documented tolerance to zero. A materially negative `M2` indicates excessive drift or a bookkeeping defect and should trigger a warning and full recalculation rather than being silently accepted.

## Replacement

Replacing the Y value at an existing X should remain one externally atomic mutation:

1. Remove the old normalized statistical contribution without raising `DataChanged`.
2. Normalize and add the new contribution without raising `DataChanged`.
3. Raise one `DataChanged` event after the series is internally consistent.

A transition between a missing/null Y and a numeric Y changes the statistical count. A transition between two null values should not alter statistical state or raise an event unless the public mutation contract treats an identical assignment as a change.

## Explicit Recalculation

Add a public operation tentatively named:

```text
RecalculateStatistics()
```

It should rebuild all derived statistical state from the normalized values currently stored in the series:

- Count and sum.
- Mean and `M2`, using forward Welford accumulation.
- Sorted value frequencies.
- Minimum and maximum.

Questions for the final API:

- Should explicit recalculation raise `DataChanged`? The underlying observations have not changed, so a separate diagnostic/statistics event or no event may be more accurate.
- Should it return a result describing whether it corrected drift or invalid state?
- Should a private recalculation path be callable automatically after detecting a materially invalid `M2`?

Initial recommendation: expose a void public method, do not raise `DataChanged`, and use the same private rebuild routine for construction and automatic recovery.

## Accuracy and Drift Policy

Forward Welford updates are stable. Inverse removal can accumulate error after long mutation sequences, especially when magnitudes differ greatly.

Initial policy:

- Use incremental inverse removal normally.
- Clamp only tiny negative `M2` values.
- Automatically recalculate if an invariant is materially violated.
- Let presentation authors or higher-level rendering code request explicit recalculation when maximum reproducibility is desired.
- Do not introduce periodic recalculation until measurements show a need.

## Future Starfall-Level Execution Configuration

A later global execution environment may describe whether Starfall is rendering interactively or producing deterministic offline/video output where additional computation is acceptable.

Potential future policies include:

- Interactive: favor incremental updates and recalculate only on invariant failure.
- Offline/high-accuracy: explicitly recalculate statistics before rendering a scene, frame range, or exported artifact when requested.
- Diagnostic: compare incremental state with a fresh calculation and report drift.

`DataSeries` should not depend on this global configuration in the initial implementation. Providing a deterministic explicit recalculation method is enough to support such policy later without committing to a global service design now.

## Bucket and Source Composition

Welford summaries are mergeable. If future resolved buckets expose count, mean, and `M2`, two summaries can be combined without enumerating their observations:

```text
delta = meanB - meanA
count = countA + countB
mean  = meanA + delta * countB / count
M2    = M2A + M2B + delta^2 * countA * countB / count
```

This is relevant to generated, cached, analytical, and parallel data sources, but should not expand the initial `DataSeries` implementation scope.

## Implementation Sequence

1. Centralize Y normalization so stored and accumulated values cannot diverge.
2. Introduce mean and `M2` fields and define empty-state invariants.
3. Implement forward addition.
4. Implement inverse removal and special cases.
5. Route replacement through silent remove/add operations followed by one event.
6. Implement the shared full-recalculation routine and public entry point.
7. Switch variance and standard-deviation properties to `M2`.
8. Remove `sumOfSquares` after all consumers are migrated.
9. Add invariant checks and the materially-negative-`M2` recovery path.
10. Later, evaluate whether resolved bucket types should carry mergeable Welford summaries.

## Cases to Verify

- Empty series.
- One value and sample-variance rejection.
- Two values followed by removal of either value.
- Repeated identical values.
- Values with very large magnitude but very small variance.
- Alternating additions and removals.
- Replacement: numeric to numeric, numeric to null, null to numeric, and null to null.
- NaN and both infinities normalized to zero with only one warning.
- Incremental state compared with `RecalculateStatistics` after a long mutation sequence.
- Population and sample variance compared with known examples.
