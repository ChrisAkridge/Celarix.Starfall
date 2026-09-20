# Charts Low-Priority Notes

This document records accepted limitations, possible refinements, and performance risks that are worth remembering but are not currently scheduled for implementation.

## Document Sampled-Aggregate Semantics

`StandardResolutionStrategy` currently uses the same aggregate result shape for exact stored observations and sampled generated functions. For a sampled source:

- Count is the sample count, not necessarily the population count.
- Sum is the sample sum, not a population sum.
- Minimum and maximum are observed sample extrema, not proven population extrema.
- Average is a sample estimate unless the source or strategy establishes exactness.

Proving extrema for an arbitrary function generally requires a full scan or additional mathematical/source-specific knowledge. An eventual public documentation pass should state these tradeoffs clearly rather than implying that generated-function aggregates are rigorous population statistics.

A future result model may carry exactness or provenance metadata, but no redesign is scheduled solely for this documentation concern.

## Even-Sampling Aliasing

`XRange.Sample` chooses deterministic evenly spaced X values. This gives reproducible spatial coverage but can systematically miss or misrepresent:

- Narrow spikes between sample positions.
- Periodic functions aligned with the sampling interval.
- Alternating or highly oscillatory values.
- Discontinuities between chosen X locations.

Possible future strategies include deterministic stratified pseudo-random samples, mandatory endpoints and midpoint, adaptive subdivision, or source-specific samplers. Any change should preserve deterministic frame rendering and clearly distinguish visual sampling from statistical guarantees.

## Request-Scoped Sampling Budgets

`FunctionDataSource` currently fixes `samplesPerBucket` at construction. A future resolution request may need to vary the budget according to:

- Interactive versus offline-quality rendering.
- Bucket width and zoom level.
- Display resolution.
- Source behavior or strategy.
- A global or presentation-level quality policy.

The existing request/context types may provide a starting point, but the design should wait until rendering-policy requirements are clearer.

## Temporary Allocation Pressure

Current resolution creates per-bucket lists or arrays of `DataPoint` values and then creates resolved-result collections. Exact aggregation of a large visible series during repeated invalidations may therefore create significant temporary allocation and garbage-collection pressure.

Possible future responses include:

- Streaming observations directly into a resolution accumulator.
- Reusing or pooling buffers.
- Allowing a source to return an aggregate without materializing every `DataPoint` in a bucket.
- Reusing resolved buckets across viewport changes.

Do not implement these optimizations before the Phase 2 performance baseline identifies allocation as a meaningful cost.
