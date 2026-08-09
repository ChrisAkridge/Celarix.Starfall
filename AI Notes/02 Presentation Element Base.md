# Presentation Element Base

Many custom elements need the same small amount of infrastructure: an `AnimationContext`, scheduling helpers, and common animation conveniences.

A base class above `AtriaElement` could provide this without changing the core rendering model. Another option is to change `AtriaElement` directly while the codebase is still small enough to migrate. With roughly 30 slides, now is a good time to improve the core shape before the presentation authoring style hardens.

Possible shape:

```csharp
public abstract class AtriaElement
{
    protected AnimationContext Animations { get; }

    protected void Schedule(FixedDurationAnimation animation);
    protected int Seconds(double seconds);
}
```

With centralized animation accounting, elements probably should not create contexts directly with `new AnimationContext()`. They should receive or create contexts through an engine-owned registry/factory so the presentation can answer "is anything finite currently animating?"

Possible registry-aware shape:

```csharp
internal void SetAnimationContext(AnimationContext context)
{
    Animations = context;
}
```

or:

```csharp
protected AnimationContext Animations { get; } = Slide!.AnimationContexts.CreateFor(this);
```

This keeps custom elements expressive while removing repeated plumbing.

This cleanup also suggests smaller authoring conveniences for common element construction. The Floating Point slides repeat a lot of `TextBlock` setup: text, font family, font size, color, opacity, and measurement. A lightweight factory could make slide code less noisy without becoming a DSL.

Possible text factory shape:

```csharp
var text = TextBlockFactory
    .Consolas()
    .WithSize(48f)
    .WithColor(SColor.White);

var label = text.MakeText("#label", "Mantissa");
var bigLabel = text.MakeText("#bigLabel", "Big text").WithSize(72f);
```

The goal is to reduce initializer bulk while preserving the cheaty, fluent C# feel.

Current direction:

- The `AtriaElement` animation/context cleanup should happen in core Starfall, not only in presentation helper code.
- Existing `AtriaElement` should be changed directly while the migration surface is still manageable.
- The Floating Point talk provides enough real slide code to guide and validate the migration.
- `TextBlockFactory` and similar factories should also live in core Starfall because they improve the normal Atria authoring experience.
- Factories should be semantically equivalent to calling constructors and setting properties by hand. They are syntax relief, not a new element model.

Possible namespace:

```csharp
namespace Celarix.Starfall.Layout.Atria.Elements.Factory;
```

The namespace is descriptive, though a bit long. Alternatives could be considered if it gets painful in everyday use.

Questions to explore:

- What convenience methods are worth standardizing without making the base class too large?
- Should factories also handle measurement and initial `Size`, or leave that to render-time measurement?
- Is `Celarix.Starfall.Layout.Atria.Elements.Factory` the right namespace, or should it be shorter?
