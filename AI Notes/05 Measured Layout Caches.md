# Measured Layout Caches

Several elements measure text repeatedly. Text measurement can be expensive enough that it should be cached centrally.

That pattern is useful enough to make explicit.

Earlier element-local sketch:

```csharp
_bitSize.GetOrUpdate(_font, f => target.MeasureText("0", f));
```

In this sketch, `_bitSize.GetOrUpdate(...)` returns an `SSizeF`. The field is a small cache object, not the size itself.

Possible generic cache:

```csharp
private CachedMeasurement<SFont, SSizeF> _bitSize = new();

private SSizeF BitSize(IRenderTarget target)
{
    return _bitSize.GetOrUpdate(_font, font => target.MeasureText("0", font));
}
```

`GetOrUpdate` compares the current key to the last key. If the key matches, it returns the cached value. If the key changed, it calls the factory, stores the new key and value, then returns the new value.

Possible implementation shape:

```csharp
public sealed class CachedMeasurement<TKey, TValue>
{
    private TKey? _key;
    private TValue? _value;
    private bool _hasValue;

    public TValue GetOrUpdate(TKey key, Func<TKey, TValue> measure)
    {
        if (_hasValue && EqualityComparer<TKey>.Default.Equals(_key, key))
        {
            return _value!;
        }

        _key = key;
        _value = measure(key);
        _hasValue = true;
        return _value;
    }

    public void Invalidate() => _hasValue = false;
}
```

or a more specialized helper for text measurement:

```csharp
_measurements.Text("0", _font);
```

The specialized version may be more natural if text measurement is the common case:

```csharp
private TextMeasurementCache _measurements = new();

var bitSize = _measurements.Measure("0", _font, target);
var negativeSize = _measurements.Measure("Negative!", negativeFont, target);
```

That can cache by `(text, font)` rather than a single last key. This is more powerful, but also raises questions about cache lifetime and whether the render target affects measurement results.

Current direction:

- `MeasurementService` should own the main measurement cache.
- All `MeasurementService.MeasureText(...)` calls should check the cache first.
- The cache key should include the measured text and an `SFont` key.
- `SFont` should be made value-comparable enough, or otherwise provide a reliable key, so it can safely participate in cache keys.
- Reuse the same caching pattern already used in `SkiaTextRendering`, where `Cached<T>` provides TTL and eviction behavior.

Possible service-level shape:

```csharp
public SSizeF MeasureText(string text, SFont font)
{
    var cacheKey = $"{text}|{font.ToCacheKey()}";
    if (Cached<SSizeF>.TryGet(cacheKey, out var cachedSize))
    {
        return cachedSize;
    }

    var measuredSize = renderTarget.MeasureText(text, font);
    return cachedSize.Save(measuredSize, TimeSpan.FromMinutes(DefaultCacheDurationMinutes));
}
```

`SFont` already has a `ToCacheKey()` pattern used by `SkiaTextRendering.GetFont(...)`. That may be enough for initial measurement caching, though proper value equality on `SFont` would make cache keys and dictionaries cleaner in the long run.

`MeasurementService` is the natural home for shared caches. It is already the measurement boundary for elements and wraps the current render target. If HarfBuzz shaping data is cached around the measurement path, putting text/layout measurement caches here keeps the behavior centralized and available to all elements.

There are probably two useful levels:

- `MeasurementService`-level caches for common text measurement and font metrics requests across all elements using the same render target.
- Element-local last-value caches only for specialized layout values that are not just direct text measurement.

The goal is to keep render code readable while avoiding repeated measurement work.

Questions to explore:

- How should cache invalidation work when render targets change?
- Should measurement caches use `SFont.ToCacheKey()` strings initially, or implement full value equality first?
- Should font metrics be cached in `MeasurementService` with the same pattern?
- Should text measurement cache keys include render target identity?
