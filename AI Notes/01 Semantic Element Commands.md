# Semantic Element Commands

Custom elements should expose verbs that match the presentation concept, not their drawing mechanics.

Examples from the Floating Point presentation:

- `MoveWindowToExponent(...)`
- `SetShowPlaceValues(...)`
- `SetBitAndAdvanceArrowAndScroll(...)`
- `ShowBytes()`
- `MergeBoxes()`
- `FillImage()`

The convention would be: slide code speaks in the language of the talk, while the element owns layout, rendering, and animation details.

Current leanings:

- Semantic commands should not animate by default as a rule. Animation is part of the command only when the element author chooses that behavior.
- Semantic commands should not generally return animation handles. If scheduling or waiting is needed, that probably belongs to the animation/timeline infrastructure rather than each semantic command.
- Out-of-order calls are the element author's choice. Some elements may support arbitrary command order; others may validate aggressively because their semantics depend on a staged sequence.
- Shared command/state validation helpers may not be necessary. Most validation probably looks like ordinary guard clauses: check the relevant conditions and throw if the command is invalid.
- Presentation recovery should stay quiet and pragmatic. If a slide throws during a live talk, the runner can print the exception to the console and reconstruct the current slide rather than showing an on-screen error or crashing loudly.
- Slides should remain non-persistent and cheap to reconstruct. If a presentation author needs durable state, they can create their own model/service object and pass or reference it from reconstructed slides.

Possible validation style:

```csharp
public void MergeBoxes()
{
    if (_stage < BinaryDrawingStage.ColorBoxes)
    {
        throw new InvalidOperationException("Boxes must be colored before they can be merged.");
    }

    // Continue with the semantic command.
}
```

This keeps command validation local to the element's semantics rather than introducing a framework-level state validation system.

Error handling direction:

- Log exceptions to the console.
- Reinitialize the current slide when possible.
- Avoid on-screen error UI by default.
- Avoid making slides responsible for preserving durable state.
- Do not strongly separate slide exceptions from render-target exceptions in normal presentation flow. The practical goal is to give the presenter as many chances as they need to retry or navigate away.
- Exceptions during slide construction are more serious because the slide cannot be recovered by reinitializing itself. In that case, offer the user a choice of which slide to go to next, using the same kind of slide picker already supported by `PresentationRunner`.
- Future driver UI could expose the same recovery controls currently handled through the console.
- Debugging visuals may still be valuable later, especially for Atria layout debugging, but they are different from live presentation error display.

Recovery loop:

```text
slide fails during live presentation
  -> print exception to console
  -> stop drawing frames during fallback
  -> attempt to rebuild/reinitialize the same slide once
  -> if rebuild succeeds, resume presentation on that slide
  -> if rebuild fails, print that exception too
  -> show slide list on the console
  -> let the user choose another slide
  -> build that slide and resume
```

The fallback period should not keep rendering broken frames. The console becomes the recovery interface until a slide is successfully selected and built.

Debug visualization direction:

Atria debug visuals deserve a real subsystem rather than ad hoc flags. `AtriaLayoutEngine`, slides, and elements could each register debug visualization toggles. A central registry or panel can list available toggles and let the user enable/disable them.

Possible shape:

```csharp
DebugVisuals.Register("Atria.ShowBasisPoints", () => DebugMode.ShowBasisPoints);
DebugVisuals.Register("Slide.ShowAnchors", () => _showAnchors);
DebugVisuals.Register("FloatingPointWindow.ShowRowBounds", () => _showRowBounds);
```

or:

```csharp
public interface IDebugVisualProvider
{
    IEnumerable<DebugVisualToggle> GetDebugVisuals();
}
```

Potential debug visual sources:

- `AtriaLayoutEngine`: global layout diagnostics, frame count, current slide, animation state.
- `AtriaSlide`: basis points, anchors, layer boundaries, slide-safe areas.
- `AtriaElement`: element bounds, internal layout guides, semantic debug overlays.

Questions to explore:

- Should debug visual toggles be registered imperatively, discovered via an interface, or both?
- Should debug visuals render through normal Atria layers, a special debug layer, or direct render-target overlay calls?
- Should debug toggle state persist across slide reconstruction?
