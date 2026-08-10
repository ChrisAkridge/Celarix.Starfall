# Starfall Implementation To-Do

This list tracks implementation status across the AI Notes design threads.

## Done

- [x] MeasurementService text measurement cache from #05.
- [x] AnimationContextRegistry from #07.
- [x] AtriaSlide and AtriaElement animation contexts routed through the registry from #02/#07.
- [x] Existing slide and element AnimationContext call sites migrated to the registry-owned model.
- [x] AnimateBasic and AtriaElement property animations moved onto AnimationContext.
- [x] Central registry ticking so all element-owned animation contexts advance every frame.
- [x] AnimationSlot first pass from #04.
- [x] ForceFinish() first pass for FixedDurationAnimation and ContinuingAnimation.
- [x] FloatingPointWindowElement migrated to slots for scroll, window movement, arrow movement, arrow opacity, and negative flag opacity.

## Current Priority

- [ ] Layers, transforms, and render context as the next larger phase.
  - [ ] Define STransform2D or equivalent friendly transform helper.
  - [ ] Decide the render-target push/pop API for transform and clipping scopes.
  - [ ] Add layer model, likely LayeredAtriaSlide<TLayer>.
  - [ ] Let layers pan/zoom independently.
  - [ ] Pass viewport/render context information down to AtriaElement.Render.
  - [ ] Add geometry helpers needed for transformed viewport visibility.

## Backburner

- [ ] SteppedAtriaSlide / choreography helper from #03.
- [ ] TextBlockFactory and related constructor cleanup from #02.
- [ ] FrameScheduler from #08.

## Later Cleanup

- [ ] Remove now-redundant manual Animations.Update(...) calls from slides/elements.
- [ ] Split continuing animations into finite-until-stable vs. infinite/ambient animation concepts.
- [ ] Add ScheduleWhenAnimationsStabilize or equivalent registry/context API.
- [ ] Add tests around AnimationContextRegistry, AnimationSlot replacement policy, and ForceFinish behavior.
