# Slide Choreography Helper

Several slides are simple step machines: advance calls the current step, the step mutates visuals, then the slide moves to the next state.

The current switch-based style is clear, but it repeats a lot of state bookkeeping. The attribute-driven `StateMachine<T>` approach also proved painful; transitions are easier to understand when they are explicit C# calls in the slide's own flow.

Possible goal:

```csharp
Steps
    .Then("Fade in bit row", FadeInBitRow)
    .Then("Zoom and show labels", ZoomInAndShowLabels)
    .Then("Show window", ShowWindow)
    .Then("Ask audience", AskAudience)
    .ThenAdvance();
```

This should stay plain C#, not become a separate presentation DSL.

Step names are useful, especially when steps are registered with lambdas where `nameof(...)` is not available. Names can feed console logging, future driver UI, or debug output.

Forward-only should be the default. Most Floating Point slides already work this way, and the runner's "first Back press reinitializes the slide, second Back press goes to the previous slide" behavior is a reasonable practical answer for many talks.

Reverse transitions are their own design problem. A helper could support them, but they should be opt-in rather than required by every stepped slide.

Possible forward-only shape:

```csharp
Steps
    .Then("Fade in bit row", FadeInBitRow)
    .Then("Show labels", ShowLabels)
    .Then("Show window", ShowWindow)
    .ThenAdvance();
```

Possible opt-in reverse shape:

```csharp
Steps
    .Between("Show labels", forward: f => ShowLabels(), backward: f => HideLabels())
    .Between("Show window", forward: f => ShowWindow(), backward: f => HideWindow());
```

For more complex bidirectional slides, Starfall may not need to force the step abstraction. A slide that wants full backward navigation could receive key events and own its own interaction model.

Advance behavior should respect centralized animation accounting:

- If no finite animation is in progress, advance runs the next step immediately.
- If finite animation is in progress, the first advance request schedules an ASAP advance for when finite animations finish.
- If advance is pressed again while an advance is already queued, it may force the advance immediately.

Forcing advance implies flushing finite animations:

```text
AnimationContextRegistry.ForceFinishFiniteAnimations()
  -> call ForceFinish() on all finite animations
  -> clear them from the registry/contexts
  -> run the queued advance on that same frame
```

This gives presenters an escape hatch: one click queues the next step after motion settles; a second click says "finish the current motion now and move on."

Rewind behavior for forward-only stepped slides should match the existing runner model:

- If the slide is not at its initial state, reinitialize the current slide.
- If the slide is already at its initial state, go to the previous slide.

Questions to explore:

- Should the API be `Then`, `Between`, or another verb?
- Should forced advance always flush finite animations, or should individual animations be allowed to opt out?
- Where should queued advance live: stepped slide helper, presentation runner, timeline, or layout engine?
