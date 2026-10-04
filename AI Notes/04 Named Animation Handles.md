# Opt-In Animation Slots

Custom elements can receive overlapping semantic commands. A later command may need to replace, finish, or cancel an earlier animation.

`FloatingPointWindowElement` already has comments pointing toward animation state slots for scrolling, moving the window, moving the arrow, and related behaviors.

This does not need to mean every animation is named and cancelable. Most animations can still be fire-and-forget:

```csharp
Animations.Schedule(FixedDurationAnimation.StartNow(...));
```

For elements that expose conflicting semantic animation surfaces, use opt-in slots owned by the element:

```csharp
private AnimationSlot _movingWindow;
private AnimationSlot _movingArrow;

public void MoveWindowToExponent(int exponent)
{
    _movingWindow.Replace(FixedDurationAnimation.StartNow(...));
}

public void SetArrowBit(int exponent)
{
    _movingArrow.Replace(FixedDurationAnimation.StartNow(...));
}
```

The slot is not primarily a global name. It is a small local control surface for "the current animation responsible for this semantic property." It may have an optional debug name, but callers should not have to name every animation they schedule.

Possible slot operations:

```csharp
slot.Replace(animation);
slot.Cancel();
slot.FinishNow();
slot.Dispose();
slot.IsRunning
```

Replacement policy should be configurable, but the default should be force-finish. Animations may be doing more than visual interpolation; they may set final state or run `onCompleted` handlers. Force-finishing the old animation before replacing it is the safest default.

Possible creation:

```csharp
_movingWindow = Animations.CreateSlot(this, "window.move");
```

or:

```csharp
_movingWindow = new AnimationSlot(Animations, "window.move");
```

The preferred creation path is probably through `AnimationContext`, so slots are tracked by the same context and registry as normal animations:

```csharp
_movingWindow = Animations.CreateSlot(debugName: "window.move");
```

Slot names should be optional debug labels, not addressing keys. The real identity of a slot should usually be the field that owns it:

```csharp
private AnimationSlot _movingWindow;
```

Each slot should track exactly one animation. If an element needs an indeterminate number of independently replaceable animations, it can create slots in a loop or collection. That implies slots need a destruction/disposal path so temporary slots do not remain registered forever.

The important part is that the slot still schedules through the element's `AnimationContext`, so centralized animation accounting continues to work.

Current direction:

- Slots are opt-in.
- Slots are created by `AnimationContext`.
- Slot names are optional debug labels.
- Slot identity should come from fields/references, not string keys.
- A slot tracks exactly one animation.
- Replacing an animation force-finishes by default.
- Replacement behavior should still be configurable per slot or per replacement call.
- Slots should be disposable/destroyable.

Questions to explore:

- Should disposal force-finish the current animation, cancel it, or require an explicit policy?
- Should temporary slots be common enough to need collection helpers?
