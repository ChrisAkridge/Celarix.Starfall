# Centralized Animation Accounting

Animation authorship should stay local, but animation accounting should be centralized.

Slides and elements should still schedule expressive animations close to the semantic action that owns them. The presentation/layout engine needs a way to ask the whole current presentation whether anything relevant is animating.

This helps two major cases:

- Interactive advance: if something is animating, should advance happen now or be queued until idle?
- PNG rendering: how many frames should be rendered before the scene is considered complete?

Possible shape:

```csharp
public sealed class AnimationContext
{
    public bool IsAnimating { get; }
    public int ActiveAnimationCount { get; }
    public int? LastScheduledEndFrame { get; }

    public void ScheduleWhenAnimationsStabilize(Action callback);
}
```

```csharp
public sealed class AnimationContextRegistry
{
    public AnimationContext CreateContext(object owner);
    public bool IsAnythingAnimating { get; }
    public int ActiveAnimationCount { get; }
    public int? LatestScheduledEndFrame { get; }
}
```

The registry should be owned by an engine or presentation instance, not by a singleton. Unit tests can then create a fresh registry, make contexts, schedule animations, and assert global animation state without shared static state leaking between tests.

Possible ownership:

```text
AtriaLayoutEngine
  owns AnimationContextRegistry
    creates slide context
    creates element contexts
    can answer IsAnythingAnimating
```

Contexts need lifecycle management. Slides are intentionally cheap and non-persistent, and the runner may reconstruct them often. When a slide is removed or remade, its animation contexts and element contexts should be destroyed so the registry does not accumulate dead contexts forever.

Possible direction:

```csharp
public abstract class AtriaSlide : IDisposable
{
    public virtual void Dispose()
    {
        AnimationContexts.DisposeOwnedBy(this);
        foreach (var element in Elements)
        {
            element.Dispose();
        }
    }
}

public abstract class AtriaElement : IDisposable
{
    public virtual void Dispose()
    {
        Animations.Dispose();
    }
}
```

The goal is for normal slide and element authors not to implement disposal just to clean up animation accounting. They should only override disposal when they own additional resources.

This also points toward porting `AnimateBasic` onto `AnimationContext`. Right now `AnimateBasic` uses slide-owned `ActiveAnimation`s, while newer code uses local `AnimationContext`s. Until those are unified, a global animation query has to check both animation systems.

Animation duration categories probably need to be explicit:

- Known finite animations: fixed duration, known end frame.
- Unknown finite animations: indeterminate at schedule time, but expected to complete.
- Infinite animations: ambient or looping animations that should not block advancement or prevent the PNG renderer from finishing.

The current `ContinuingAnimation` type mixes at least the last two categories. It should probably be split, renamed, or given explicit metadata so the engine can distinguish "wait for this to finish" from "this can run forever in the background."

The unknown-finite category means: the animation will finish, but it decides frame-by-frame whether it is done. Each update answers either "I am done" or "I need at least one more frame."

Naming candidates:

- `PredicateAnimation`: accurate mechanically, but bland and describes implementation more than intent.
- `CompletingAnimation`: emphasizes that it is expected to complete.
- `OpenEndedAnimation`: captures unknown duration, but might imply it may never end.
- `ConditionalAnimation`: says completion is condition-driven, but does not imply finite.
- `FiniteContinuingAnimation`: explicit, but clunky.
- `UntilCompleteAnimation`: clear and fairly Starfall-friendly: it keeps running until it reports completion.

Current preference: `UntilCompleteAnimation`.

Current direction:

- Infinite animations should be ignored by `IsAnythingAnimating`.
- Infinite animations should still be queryable separately, likely through a count or read-only list.
- "Stable" means no finite animations are running. Infinite/ambient animations may still be active.
- Queued advance should probably be owned by the animation/timeline context, not by each slide.
- A possible API is `AnimationContext.ScheduleWhenAnimationsStabilize(Action callback)`.
- `ActiveAnimation` should disappear to reduce confusion between old and new animation systems.
- Disposing an `AnimationContext` should force-finish finite animations. Authors should get a final 100% progress update and `onCompleted` should run.
- Exceptions thrown while force-finishing during disposal should be caught/logged so they do not prevent the context from finishing disposal.
- All finite animation types should expose `ForceFinish()`.
- The default `ForceFinish()` behavior can be equivalent to setting progress to `1.0`, running completion logic, and marking the animation completed.
- More complex animations can override `ForceFinish()`. For example, a staggered animation should schedule all remaining child animations, force-finish them immediately, run their completions, then complete itself.

Possible base shape:

```csharp
public abstract class Animation
{
    public bool Completed { get; protected set; }

    public abstract void Update(int currentFrame);

    public virtual void ForceFinish()
    {
        UpdateProgress(1.0d);
        OnCompleted?.Invoke();
        Completed = true;
    }
}
```

Questions to explore:

- Is `UntilCompleteAnimation` the right name for the unknown-finite category?
- Is `ScheduleWhenAnimationsStabilize(...)` the right name for "run when only infinite/ambient animations remain"?
- What should the abstract/base animation API look like so `ForceFinish()` is natural for both fixed-duration and until-complete animations?
