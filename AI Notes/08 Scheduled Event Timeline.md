# Frame Scheduler

Starfall has a deliberately discrete model of time: frames advance one by one. Animation scheduling is one use of that model, but the broader idea may be a scheduled event timeline.

This would let slides, elements, or the presentation engine schedule non-animation work against frame time.

Possible operations:

```csharp
FrameScheduler.RunAtFrame(frameNumber, action);
FrameScheduler.RunInFrames(30, action);
FrameScheduler.RunWhen(predicate, action);
```

`RunAfterFiniteAnimations` / `ScheduleAtEndOfFiniteAnimations` should likely stay on `AnimationContext`, because that behavior is directly tied to animation accounting. `FrameScheduler` should be separate from `AnimationContext` and focus on non-animation frame scheduling.

This keeps semantic element commands simple. A command does not need to animate by default, return handles, or know who is waiting on it. Scheduling and waiting live in context services.

Potential uses:

- Trigger a callback a fixed number of frames after a visual event.
- Delay a state transition without creating a dummy animation.
- Run non-animation work on a future frame.
- Poll a non-animation predicate until it becomes true.

Scheduled callbacks should not be able to schedule more callbacks for the same frame. They may schedule work for future frames. If they have more work to do on the current frame, they should just do it immediately within the current callback.

Error handling should match the rest of live presentation behavior:

- Let uncaught callback exceptions flow to the normal slide error handling path.
- Reinitialize the slide when possible.
- Keep giving the presenter chances to retry or pick another slide.
- Treat only truly fatal failures as unrecoverable.

Current direction:

- Name: `FrameScheduler`.
- Keep `FrameScheduler` separate from `AnimationContext`.
- Keep animation-idle scheduling on `AnimationContext`.
- Disallow same-frame scheduling from scheduled callbacks.

Open questions:

- Should `FrameScheduler` be owned by `AtriaSlide`, `AtriaLayoutEngine`, or another context object?
- Should `RunWhen(predicate, action)` have a timeout or maximum frame count?
- Should scheduled callbacks have optional debug labels?
