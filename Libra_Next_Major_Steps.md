# Starfall / Libra — Next Major Steps

This is a practical continuation plan for Libra after the July 2026 work on expressions, metrics, scripts, rows, fractions, and fenced expressions.

The goal is not to design every future feature now. It is to leave enough structure that future work can begin without reconstructing the architecture from memory.

## Current baseline

Libra currently has a healthy retained-mode layout model:

```text
LibraExpression
    ↓
Layout(...)
    ↓
LibraLayoutResult
    ↓
LibraRenderable
    ↓
IRenderTarget
    ↓
Skia target
```

Important existing concepts include semantic expression trees, stable expression and renderable identities, persistent replacement, recursive layout, normalized local bounds, baseline and math-axis alignment, metrics records, text, binary expressions, fractions, fenced expressions, scripts, rows/concatenation, and structural animation.

The next major phase is to add general vector geometry without letting Libra become an SVG clone.

# 1. Wire line, path, and curve drawing through the rendering stack

## Objective

Allow Libra renderables to describe arbitrary stroked or filled geometry while keeping Skia-specific types out of Libra itself.

Expected path:

```text
Libra path renderable
    ↓
IRenderTarget
    ↓
Skia*Target
    ↓
SkiaCommon
    ↓
SKCanvas / SKPath
```

Most of this should be straightforward passthrough work.

## 1.1 Confirm the retained path model

**Complete.**

A backend-independent path can consist of commands such as:

```csharp
public abstract record LibraPathCommand;

public sealed record MoveTo(double X, double Y)
    : LibraPathCommand;

public sealed record LineTo(double X, double Y)
    : LibraPathCommand;

public sealed record QuadraticTo(
    double ControlX,
    double ControlY,
    double X,
    double Y)
    : LibraPathCommand;

public sealed record CubicTo(
    double Control1X,
    double Control1Y,
    double Control2X,
    double Control2Y,
    double X,
    double Y)
    : LibraPathCommand;

public sealed record ClosePath
    : LibraPathCommand;
```

And a path value:

```csharp
public sealed record LibraPath(
    IReadOnlyList<LibraPathCommand> Commands);
```

Do not add arc support until an actual symbol needs it. Cubic curves can cover a great deal.

## 1.2 Add path styling

**Complete.**

A path renderable needs explicit stroke and fill information.

```csharp
public sealed record LibraPathStyle
{
    public SColor? FillColor { get; init; }
    public SColor? StrokeColor { get; init; }
    public double StrokeWidth { get; init; }
    public LibraStrokeCap StrokeCap { get; init; }
    public LibraStrokeJoin StrokeJoin { get; init; }
}
```

Suggested enums:

```csharp
public enum LibraStrokeCap
{
    Butt,
    Round,
    Square
}

public enum LibraStrokeJoin
{
    Miter,
    Round,
    Bevel
}
```

A path may be stroke-only, fill-only, or both.

## 1.3 Add a path renderable

**Complete.**

Possible shape:

```csharp
public sealed class LibraPathRenderable : LibraRenderable
{
    public required LibraPath Path { get; init; }
    public required LibraPathStyle Style { get; init; }
}
```

Prefer this positioning model:

```text
Path coordinates are local to the renderable, and Position translates them.
```

That is clean for cloning, animation, and translation. The renderable must report bounds that include stroke thickness.

## 1.4 Extend `IRenderTarget`

**Complete.**

The simplest useful API may be one retained call:

```csharp
void DrawPath(
    LibraPath path,
    SPointF position,
    LibraPathStyle style);
```

Keep the interface backend-independent. Do not expose `SKPath`, `SKPaint`, or `SKCanvas` above the Skia-specific layer.

## 1.5 Implement the Skia passthrough

**Complete.**

The Skia side should mostly translate Libra commands directly:

```csharp
private static SKPath CreateSkiaPath(LibraPath path)
{
    var skPath = new SKPath();

    foreach (var command in path.Commands)
    {
        switch (command)
        {
            case MoveTo move:
                skPath.MoveTo((float)move.X, (float)move.Y);
                break;

            case LineTo line:
                skPath.LineTo((float)line.X, (float)line.Y);
                break;

            case QuadraticTo quadratic:
                skPath.QuadTo(
                    (float)quadratic.ControlX,
                    (float)quadratic.ControlY,
                    (float)quadratic.X,
                    (float)quadratic.Y);
                break;

            case CubicTo cubic:
                skPath.CubicTo(
                    (float)cubic.Control1X,
                    (float)cubic.Control1Y,
                    (float)cubic.Control2X,
                    (float)cubic.Control2Y,
                    (float)cubic.X,
                    (float)cubic.Y);
                break;

            case ClosePath:
                skPath.Close();
                break;
        }
    }

    return skPath;
}
```

Then configure `SKPaint` from `LibraPathStyle`.

## 1.6 Add a path builder helper

**Complete.**

Raw command lists will become annoying quickly.

```csharp
var path = new LibraPathBuilder()
    .MoveTo(0, 10)
    .LineTo(4, 14)
    .LineTo(9, 2)
    .LineTo(30, 2)
    .Build();
```

Useful methods:

```csharp
MoveTo(...)
LineTo(...)
QuadraticTo(...)
CubicTo(...)
Close()
Build()
```

Avoid creating a large mini-language until repeated symbol code demonstrates the need.

## 1.7 Verify translation and animation

Before drawing mathematical symbols, test:

- moving a path renderable
- cloning a path renderable
- bounds after translation
- fade-in and fade-out
- position interpolation
- size interpolation
- stroke color interpolation
- stroke width interpolation

Path morphing is not required yet. A path can initially animate like any other renderable.

# 2. Define procedural symbols effectively

## Objective

Create good-looking mathematical geometry from a small shared set of metrics rather than hand-tuning every symbol independently.

Immediate targets:

- radical signs
- tall parentheses
- brackets
- angle brackets
- vertical bars
- hats
- overlines
- arrows

Braces can wait until the basic path system feels comfortable.

## 2.1 Establish shared drawing metrics

**Complete.**

Add metric groups when implementing their first consumer.

```csharp
public sealed record LibraMathMetrics
{
    public ScriptMetrics Scripts { get; init; } = new();
    public FractionMetrics Fractions { get; init; } = new();
    public FenceMetrics Fences { get; init; } = new();
    public StrokeMetrics Strokes { get; init; } = new();
    public RadicalMetrics Radicals { get; init; } = new();
    public AccentMetrics Accents { get; init; } = new();
}
```

Suggested shared stroke metrics:

```csharp
public sealed record StrokeMetrics
{
    public double ThinStrokeEm { get; init; } = 0.025d;
    public double MediumStrokeEm { get; init; } = 0.04d;
    public double ThickStrokeEm { get; init; } = 0.06d;
}
```

These values become part of Libra's house-style grammar.

## 2.2 Build a symbol gallery / diagnostic page

The gallery should render a permanent reference set repeatedly.

Suggested samples:

```text
()
[]
{}
<>
|x|
||x||

sqrt(x)
sqrt(fraction)

hat(x)
hat(x + y)
overline(x + y)
vector(v)

sum with limits
integral with limits
```

Useful controls:

- font family
- font size
- zoom
- background and foreground
- procedural-vs-glyph threshold
- stroke width
- fence width and padding
- radical padding
- accent gap

Useful overlays:

- bounds
- baseline
- math axis
- renderable IDs
- path control points
- script attachment point
- glyph fallback indicator

A metric-sweep view would be especially useful:

```text
Parenthesis width:
0.20em  0.24em  0.28em  0.32em

Stroke width:
0.025em  0.035em  0.045em

Control-point bias:
0.30  0.40  0.50  0.60
```

The goal is not to guess perfect values. It is to make visual comparison cheap.

## 2.3 Glyph fallback versus procedural drawing

Use a hybrid strategy.

For short, ordinary expressions, use a font glyph. For tall or wide expressions, use procedural geometry.

The child expression should never be rewritten to inject combining Unicode characters. `Hat(Text("x"))` remains a semantic accent expression. The accent itself may be rendered as either a glyph placed above the child or a procedural path placed above the child.

## 2.4 Generalize `ParenthesizedExpression` into `FencedExpression`

**Complete.**

Likely shape:

```csharp
public sealed class FencedExpression : LibraExpression
{
    public LibraExpression Expression { get; }
    public FenceKind LeftFence { get; }
    public FenceKind RightFence { get; }
}
```

Possible kinds:

```csharp
public enum FenceKind
{
    None,
    Parenthesis,
    Bracket,
    Brace,
    Angle,
    Bar,
    DoubleBar,
    Floor,
    Ceiling
}
```

Helpers preserve a pleasant API:

```csharp
Paren(expr)
Bracket(expr)
Brace(expr)
Angle(expr)
Absolute(expr)
Norm(expr)
Floor(expr)
Ceiling(expr)
```

The layout algorithm remains:

```text
layout child
choose fence geometry
align fences to child math axis
add inner margins
normalize
inherit child baseline and math axis
```

For short fences, use glyphs. For tall fences, ask a geometry factory to create path renderables.

## 2.5 Basic procedural fence geometry

### Brackets

Three line segments. Metrics: height, width, stroke width, arm lengths, and vertical padding.

### Angle brackets

Two diagonal segments. Metrics: height, width, stroke width, and center relative to the math axis.

### Vertical bars

One or two vertical lines. Metrics: height, inter-bar gap, stroke width, and vertical padding.

### Parentheses

**Complete.**

Use cubic Bézier curves only when needed. Important geometry:

- top endpoint
- bottom endpoint
- innermost midpoint
- upper and lower control points
- width
- stroke width

Center the shape optically around the child math axis. Procedural parentheses do not need to imitate the active font exactly; they should match Libra's house style.

### Braces

Delay these until path rendering, parentheses, shared stroke metrics, and the symbol gallery are all working.

## 2.6 Radical expression

Suggested semantic shape:

```csharp
public sealed class RadicalExpression : LibraExpression
{
    public LibraExpression Radicand { get; }
    public LibraExpression? Index { get; }
}
```

Layout outline:

```text
layout radicand
determine required sign height
compute left and vertical padding
draw root hook
draw rising diagonal
draw overbar
place radicand under overbar
optionally place root index
normalize
```

Useful metrics:

```csharp
public sealed record RadicalMetrics
{
    public double HorizontalPaddingEm { get; init; }
    public double VerticalPaddingEm { get; init; }
    public double HookWidthEm { get; init; }
    public double HookDepthEm { get; init; }
    public double RisingStrokeWidthEm { get; init; }
    public double OverbarGapEm { get; init; }
    public double OverbarThicknessEm { get; init; }
    public double IndexScale { get; init; }
    public double IndexHorizontalOffsetEm { get; init; }
    public double IndexVerticalOffsetEm { get; init; }
}
```

The first radical can be built entirely from line segments. A slightly angular geometric radical may suit Starfall well.

## 2.7 Accent expression

Possible shape:

```csharp
public sealed class AccentExpression : LibraExpression
{
    public LibraExpression Expression { get; }
    public AccentKind Accent { get; }
}
```

Possible kinds:

```csharp
public enum AccentKind
{
    Overline,
    Underline,
    Hat,
    Tilde,
    Vector,
    Dot,
    DoubleDot,
    Overbrace,
    Underbrace
}
```

Layout outline:

```text
layout child
select glyph or procedural accent
size accent from child width
place above or below child
preserve child baseline and math axis
expand bounds
normalize
```

Start with overline, underline, hat, and vector. Leave tildes and braces until later.

# 3. Add cheaty semantic helper methods

## Objective

Make common mathematical constructions pleasant to author without forcing every helper to become a new rendering algorithm.

The helper layer should preserve semantic meaning where useful while internally reusing existing expression types.

## 3.1 Function invocation

Desired call:

```csharp
Function(
    Text("sin"),
    Text("x"))
```

Rendered as `sin(x)`.

A real `FunctionExpression` is useful because it gives semantic children:

- name
- argument
- whole call

Its layout may internally reuse a row and fenced expression.

Possible API:

```csharp
Function(
    LibraExpression name,
    LibraExpression argument,
    string? id = null)
```

Callers may identify semantic children directly:

```csharp
Function(
    Text("sin", "#name"),
    Text("x", "#argument"),
    "#call")
```

Generated wrappers should receive deterministic internal identities. Callers should not have to name every row or fence unless they need to target it explicitly.

## 3.2 Deterministic IDs for generated structure

For any semantic helper that creates hidden wrappers, derive identities from the owning expression:

```text
(functionGuid, "row")
(functionGuid, "argument-fence")
(functionGuid, "left-fence")
(functionGuid, "right-fence")
```

Avoid creating fresh random GUIDs during each `Layout()` call.

Safe approaches:

1. Persist generated child expressions inside the semantic expression.
2. Derive stable internal IDs from the owning expression ID and a role.

The second fits Libra's existing renderable-role model well.

## 3.3 Common helpers worth adding

### Function invocation

```csharp
Function(name, argument)
```

### Juxtaposition / multiplication

```csharp
Concat(a, b, c)
```

Examples:

```csharp
Concat(Text("2"), Text("x"))
Concat(Text("x"), Paren(...))
```

### Powers and scripts

```csharp
Pow(@base, exponent)
Subscript(@base, subscript)
Scripts(@base, superscript, subscript)
```

### Fences

```csharp
Paren(expr)
Bracket(expr)
Brace(expr)
Angle(expr)
Absolute(expr)
Norm(expr)
Floor(expr)
Ceiling(expr)
```

### Fractions and roots

```csharp
Frac(numerator, denominator)
Sqrt(expr)
Root(index, expr)
```

### Named operators

Even with upright variables, helpers can carry useful semantics:

```csharp
NamedOperator("sin")
NamedOperator("log")
NamedOperator("lim")
```

This may simply be a styled `TextExpression` or a semantic wrapper.

### Equality and relation helpers

```csharp
Eq(left, right)
DefinedAs(left, right)
Approx(left, right)
LessThan(left, right)
```

Most can still construct `BinaryExpression`.

### Grouped rows

```csharp
Row(children...)
Concat(children...)
SpacedRow(gapEm, children...)
```

### Differential helpers

Add only when needed:

```csharp
Derivative(numerator, variable)
PartialDerivative(...)
Integral(integrand, variable, lower, upper)
```

## 3.4 Keep helper semantics bounded

Use a real semantic wrapper when at least one is true:

- callers want to identify meaningful child roles
- replacement should target semantic parts
- animation should understand the structure
- layout policy may evolve independently
- the helper occurs often enough to justify a named abstraction

Otherwise, return an ordinary composition.

# 4. Additional major work likely to matter

## 4.1 Large operators

Examples: sums, products, integrals, limits, unions, and intersections.

Likely semantic structure:

```csharp
public sealed class LargeOperatorExpression : LibraExpression
{
    public LibraExpression Operator { get; }
    public LibraExpression? UpperLimit { get; }
    public LibraExpression? LowerLimit { get; }
    public LibraExpression? Operand { get; }
}
```

This reuses script concepts but centers limits above and below the operator instead of attaching them at the right.

A future distinction may be:

```text
display style: limits above and below
inline style: limits beside operator
```

Math styles can remain deferred until nested notation makes them necessary.

## 4.2 Matrix / table / aligned layout

Keep this intentionally bounded. Starfall does not need browser-grade table negotiation.

Reasonable modes:

- fixed column widths
- widest-child-per-column
- explicit author-controlled widths
- overflow diagnostics

Avoid arbitrary automatic wrapping unless a real presentation needs it.

## 4.3 Logical bounds versus visual bounds

Animation already exposed this distinction.

Eventually consider:

```csharp
LogicalBounds
VisualBounds
```

Logical bounds support parent layout, anchor placement, and stable expression size. Visual bounds support transitional renderables, clipping, invalidation, and temporary motion.

Do not derive logical size from the full transitional renderable set.

## 4.4 Script attachment points

The current default of `(Bounds.Right, BaselineY)` is sensible.

Later, expressions may optionally expose:

```csharp
SPointF RightScriptAttachmentPoint
```

Possible specialized behavior includes closing-delimiter shoulders, fractions, large operators, radicals, and decorated expressions.

This is a refinement, not a blocker.

## 4.5 Math-style propagation

Deferred, but likely future shape:

```csharp
public enum LibraMathStyle
{
    Display,
    Text,
    Script,
    ScriptScript
}
```

The rendering context can flow style downward with something like:

```csharp
context.WithMathStyle(...)
```

This may eventually affect font size, fraction spacing, script placement, operator sizing, delimiter thresholds, and limit placement. The current context model already supports this future change.

## 4.6 Font fallback

Calibri, Cambria Math, Lucida Handwriting, and Jokerman have already demonstrated that glyph coverage varies.

A future font strategy may be:

```text
primary font
    ↓ missing glyph
math-symbol fallback
    ↓ missing glyph
visible replacement glyph
```

Keep fallback deterministic so layout does not unexpectedly change between runs.

## 4.7 Regression gallery

The symbol gallery should also become a layout regression suite.

Suggested permanent expressions:

```text
x + y

4
─ + y
2

(x + y)^2
(x + y)^2_3

fraction as superscript
fraction as subscript

x(x + 1)

sumtorial(x) defined as x(x + 1) / 2

nested scripts
nested fractions
fences around fractions
mixed fonts
missing glyph
```

Useful stress-test fonts:

- Cambria Math
- Calibri
- Lucida Handwriting
- Jokerman

Jokerman is not a supported aesthetic. It is now a resilience test.

# Suggested order of work

## Next session

1. Add `LibraPathRenderable`.
2. Add path and style records.
3. Wire `DrawPath(...)` through `IRenderTarget`.
4. Implement the Skia command passthrough.
5. Draw one simple test path.
6. Verify translation, bounds, cloning, and animation.

## Following session

1. Build the symbol gallery.
2. Add shared stroke metrics.
3. Generalize `ParenthesizedExpression` into `FencedExpression`.
4. Implement procedural brackets, angle brackets, and bars.
5. Add glyph/procedural threshold logic.

## After that

1. Implement `RadicalExpression`.
2. Add radical metrics.
3. Add `AccentExpression`.
4. Implement overline, underline, hat, and vector.
5. Add function and other semantic helper expressions.
6. Begin large operators.

# Architectural principles to preserve

```text
Expressions do not render directly.
```

```text
Context flows downward.
Geometry and alignment metrics flow upward.
```

```text
Parents compose child layout results.
```

```text
Public IDs belong to semantic nodes.
Generated mechanical structure gets deterministic internal identity.
```

```text
Use glyphs where they are strong.
Use procedural geometry where glyphs cannot adapt.
```

```text
Libra should remain intentionally bounded.
It does not need to become HTML, SVG, TeX, or a browser layout engine.
```

```text
Correct geometry first.
Cheap visual iteration second.
House style emerges from consistent metrics.
```

# Immediate next action

Open with the smallest possible end-to-end path test:

```text
LibraPathRenderable
    ↓
IRenderTarget.DrawPath
    ↓
Skia target
    ↓
SKPath
    ↓
one visible diagonal line
```

Once that line appears, the rest of the symbol work becomes ordinary Libra composition.
