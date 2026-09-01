# Sparse and Generated Data Resolution

## Problem

Chart data can come from sources with very different shapes:

- A stored `DataSeries` may contain a few observations whose `BigInteger` X values are extremely far apart.
- A generated source such as `f(x) = x^2` is conceptually defined at every integer across an enormous or effectively unbounded domain.
- A future source may be able to summarize a range analytically without enumerating or sampling every X value.

The display should be able to request a bounded amount of renderable information without assuming that every integer in the visible range can or should be visited. At the same time, source-specific knowledge should not leak into chart rendering.

## Current Tension

An integer loop from `range.Minimum` through `range.Maximum` works for small contiguous ranges, but becomes unusable for sparse data with distant keys. Replacing that loop with enumeration over stored keys fixes sparse series, but does not address generated functions: those sources do not have stored keys and may represent more points than can ever be materialized.

This suggests that range lookup and resolution are source responsibilities rather than universal collection operations.

## Desired Properties

- Work should be bounded primarily by the requested output resolution, not by the numeric width of the visible range.
- Stored sparse sources should visit only observations relevant to the requested range.
- Generated sources should choose samples appropriate to each requested bucket without materializing their domain.
- Sources capable of exact or analytical summaries should be able to provide them.
- Empty ranges and missing observations must remain distinguishable from observations whose normalized Y value is zero.
- Resolution must remain deterministic for framewise rendering.
- `BigInteger` ranges must not be converted wholesale to `int` or `double` in ways that lose location information.
- Displays should consume a common resolved representation and remain unaware of how a source obtained it.

## Possible Direction

Keep bucket-oriented requests as the shared boundary. A display or resolver divides the visible viewport into a bounded number of X buckets. Each data source resolves a bucket according to its own capabilities:

- `DataSeriesDataSource` performs an ordered range query over stored X keys and summarizes only matching observations.
- `FunctionDataSource` deterministically samples a requested number of X values within the bucket.
- An analytical source may compute count, extrema, sum, mean, or other supported summaries without enumeration.
- A streaming or remote source may use an existing aggregate, cache, or index.

The common contract may eventually need to express source capabilities and resolution metadata rather than exposing only a single `ResolveBucket` operation.

## Data-Series Index Options

The stored-series implementation needs efficient mutation, point lookup, and range lookup. Candidates include:

- `SortedDictionary<BigInteger, double?>`: straightforward ordered traversal and mutation, though locating the beginning of an arbitrary range may not provide the desired asymptotic behavior through its public API.
- `SortedList<BigInteger, double?>`: binary-searchable contiguous storage with inexpensive indexed range reads, but costly insertion and removal.
- A dictionary for direct lookup plus a separate sorted key structure: flexible but requires carefully maintaining two indexes.
- A custom balanced tree or third-party ordered map with lower-bound/range enumeration support: best query semantics, at the cost of implementation or dependency complexity.

The right choice depends on expected mutation frequency, typical series size, and how often visible windows move.

## Questions to Resolve

1. Who chooses bucket boundaries: the display, a shared resolver, or the data source?
2. Should bucket count correspond exactly to pixels in dense mode, or may a display request another resolution?
3. How does a source report the difference between exact aggregation and sampled approximation?
4. Should a resolution request include sampling budgets, neighboring context, or desired statistics?
5. Which statistics must every resolved bucket provide, and which should be optional capabilities?
6. For function sources, should sampling include endpoints, midpoint, extrema heuristics, or a configurable strategy?
7. How should discontinuities, exceptions, NaN, and infinity be represented before the series/source warning policy normalizes them?
8. Can resolved buckets be cached across small viewport movements or zoom changes, and what invalidates that cache?
9. How should partially visible boundary buckets be represented in non-dense mode?
10. Are X values always discrete integer locations, or will a future source require continuous-domain semantics?

## Scenarios to Evaluate

Before choosing an API or index, compare candidate designs against at least these cases:

- Ten stored points spread across a range wider than `Int64`.
- Ten million densely stored points with a slowly scrolling viewport.
- `f(x) = x^2` over a small range and over a range millions of integers wide.
- A function with a narrow spike that naive evenly spaced sampling can miss.
- A source that can return exact aggregates for a bucket.
- Repeated zooming between individual bars and one-bucket-per-pixel dense rendering.
- Data mutations while the viewport is stationary.

## Deferred Decision

Do not optimize `DataSeries.GetPointsInRange` in isolation until the request and source contracts are settled. A local collection optimization could accidentally establish an enumeration-centered abstraction that is unsuitable for generated and analytical sources.
