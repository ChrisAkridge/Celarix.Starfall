# Schoolbook arithmetic: a from-the-basics guide

This guide covers the elements that show arithmetic the way it's taught in school:

- long addition, subtraction, multiplication and division, stacked in columns
- the addition and times table beside them, lighting up each fact as it's used
- the sine-wave "sorting video" tones that play as the work happens

It starts from the basics, including how to run Starfall at all, so it works as a refresher after time away.

---

## 1. The thirty-second version

- **`SchoolbookWork`** is the *math*. You make one with `SchoolbookWork.Addition(...)`, `.Subtraction(...)`, `.Multiplication(...)` or `.LongDivision(...)`. It works the whole problem out up front and stores it as a grid of cells plus a list of **steps**. It draws nothing.
- **`SchoolbookArithmeticElement`** is the *picture*. Give it a `SchoolbookWork` and a size, add it to a slide, and it draws the work. It can show any step: nothing done yet, partway through, or finished.
- **`OperationTableElement`** is the optional 11×11 addition or times table. Set its `Lookup` and it highlights that fact.
- **`SchoolbookSound`** turns each step into notes. It plays them through `Runtime.Tones`. On Windows the harness plugs in real speakers there, and on macOS it's silent for now.

The working demo of all of it is the **"Schoolbook Arithmetic"** slide in the **Testing / Playground** deck. Its code is in `Celarix.Starfall.Decks/Playground/MathFun/SchoolbookArithmeticSlide.cs`. When in doubt, copy from there.

---

## 2. Where everything lives

| What | File |
| --- | --- |
| The math (all four operations) | `Celarix.Starfall/Mathematics/Schoolbook/SchoolbookWork.cs` |
| The element that draws the work | `Celarix.Starfall/Atria/Elements/SchoolbookArithmeticElement.cs` |
| The addition/times table element | `Celarix.Starfall/Atria/Elements/OperationTableElement.cs` |
| Sound: the interface slides call | `Celarix.Starfall/Audio/ITonePlayer.cs` |
| Sound: steps → notes | `Celarix.Starfall/Audio/SchoolbookSound.cs` |
| Sound: note number → hertz | `Celarix.Starfall/Audio/NotePitch.cs` |
| Sound: a note's volume shape | `Celarix.Starfall/Audio/ToneShape.cs` |
| Sound: makes the actual samples | `Celarix.Starfall/Audio/ToneSynth.cs` |
| Sound: Windows speakers (NAudio) | `Celarix.Starfall/Audio/NAudioTonePlayer.cs` |
| Where the harness turns sound on | `Celarix.Starfall.Harness/PresentationController.cs` (constructor) |
| The demo slide | `Celarix.Starfall.Decks/Playground/MathFun/SchoolbookArithmeticSlide.cs` |
| The deck that lists the demo slide | `Celarix.Starfall.Decks/Playground/PlaygroundPresentation.cs` |
| Tests (also handy as examples) | `Celarix.Starfall.Tests/Mathematics/SchoolbookWorkTests.cs` |

Namespaces you'll want at the top of a slide file:

```csharp
using Celarix.Starfall;                          // SlideAdvanceResult
using Celarix.Starfall.Atria;                    // AtriaSlide, AtriaRuntime, BasisPoint, AnimationTypes
using Celarix.Starfall.Atria.Elements;           // SchoolbookArithmeticElement, OperationTableElement, TextBlock
using Celarix.Starfall.Audio;                    // SchoolbookSound, NotePitch, ToneShape
using Celarix.Starfall.Mathematics;              // Easings (for fades)
using Celarix.Starfall.Mathematics.Schoolbook;   // SchoolbookWork, MultiplicationLayout, DivisionLayout, TableLookup
using Celarix.Starfall.Rendering.Models;         // SSizeF, SPointF, SColor
using System.Numerics;                           // BigInteger, for very long numbers
```

---

## 3. Running it

You need the **.NET 10 SDK**. Check by running `dotnet --version`.

1. **Get the code.** In your copy of the repo:
   ```
   git fetch origin
   git switch master        # or whichever branch has the work
   git pull
   ```
   If you have several copies of Starfall lying around, `git branch --show-current` and `git log --oneline -3` tell you which one you're in and how up to date it is.

2. **Start the harness.** The harness is the Avalonia app that runs presentations. From the repo root:
   ```
   dotnet run --project Celarix.Starfall.Harness
   ```
   On Windows the built EXE ends up at `Celarix.Starfall.Harness\bin\Debug\net10.0\Celarix.Starfall.Harness.exe`.

3. **In the operator window** (the one with the buttons):
   - Pick a deck in **Presentation**. The demo is in **Testing / Playground**.
   - Pick a **Display** and **Size** if you care.
   - **Press Start.** Until you do, the slide list ignores double-clicks and Rewind, Reinitialize and Advance stay grayed out. It is an easy step to miss.
   - **Double-click a slide** in the list to jump to it.

4. **Moving through a slide:**
   - **Advance:** the **Advance ▶** button, or **Space**, **Right arrow** or **Page Down** while the presentation window has focus.
   - **Rewind:** **◀ Rewind**, **Left arrow** or **Page Up**.
   - **Reinitialize** rebuilds the current slide from scratch.
   - **Escape** closes the presentation window.

5. **Running the tests:** `dotnet test Celarix.Starfall.Tests`.

---

## 4. A refresher on how Starfall slides work

A quick refresher; skip ahead if it's fresh in your mind.

**A deck** is a `PresentationDefinition`: a name, a folder (just a label for grouping, like "Testing"), and a list of slides. The harness lists every deck in `Celarix.Starfall.Harness/DeckCatalog.cs`. To add a slide to the Playground deck, add a line to `PlaygroundPresentation.cs`:

```csharp
new("My Slide", runtime => new MySlide(runtime, runtime.ViewportSize))
{
    Description = "Shown in the operator window."
},
```

**A slide** is a class that inherits `AtriaSlide`. It needs:

- A constructor `(AtriaRuntime runtime, SSizeF size)` that calls `base(runtime, size)`. `size` is the viewport size, usually 1920×1080, and the slide's `Size` property holds it.
- An `Initialize()` override. This is where you build everything. It runs when the slide is shown, and again on Reinitialize.

**Elements** are the things you see: `TextBlock`, `ImageElement`, and now the schoolbook ones. To put one on screen:

1. Create it with an id string that starts with `#`, like `"#product"`. Ids just need to be unique on the slide.
2. Give it a `Size`. The schoolbook elements scale their contents to fit inside that box, so the size is all you need to control how big it looks.
3. **Anchor it.** Make a `BasisPoint` (an invisible pin at a position), then call `element.AnchorCenterTo(point)`. That puts the element's center on the pin. There are also `AnchorTopLeftTo`, `AnchorBottomCenterTo` and the rest. Useful positions on a slide include `Center`, `TopCenter` and `BottomRight`, or any `new SPointF(x, y)`.
4. **Add both** the pin and the element: `Add([anchor, element]);`. Order in the list is drawing order; later items draw on top.

To take something away, call `Remove([element]);`.

**Opacity.** Every element has an `Opacity`, and some elements, like `TextBlock`, start at **0, which is invisible**. Either set `Opacity = 1` when you create them, or fade them in with:
```csharp
Add([anchor, element]).AnimateBasic(0.5, AnimationTypes.FadeIn, Easings.Linear);
```
The schoolbook and table elements start at `Opacity = 1` so they show up right away. A fade-in still works on them.

**Beats (Advance and Rewind).** A slide can have steps inside it, which Starfall calls **beats**. Override `Advance()` and return:

- `SlideAdvanceResult.InternalStateChanged` when you did something and want to stay on this slide
- `SlideAdvanceResult.CanAdvance` when you're out of things to do, so the harness moves to the next slide

`Rewind()` is the mirror image: return `InternalStateChanged` when you stepped back, or `CanRewind` when you're at the start.

You can also override `CurrentBeat` (a name for the current beat, shown in the operator window) and `Notes` (presenter notes).

**Per-frame code.** Elements get `Update(FrameTime)` about 60 times a second. You won't need it for these elements, because auto-play uses it for you.

---

## 5. Recipes

Every snippet in this section compiles against the library as written.

### 5a. Show a finished problem (no animation)

```csharp
public sealed class FinishedProblemSlide : AtriaSlide
{
    public FinishedProblemSlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size) { }

    public override void Initialize()
    {
        BackgroundColor = new SColor(8, 0, 130, 255);

        var work = SchoolbookWork.Multiplication(487, 36);
        var element = new SchoolbookArithmeticElement("#product", work)
        {
            Size = new SSizeF(Size.Width * 0.5, Size.Height * 0.6)
        };
        element.ShowComplete();                      // jump straight to the answer

        var anchor = new BasisPoint(Center, "#productAnchor");
        element.AnchorCenterTo(anchor);
        Add([anchor, element]);
    }
}
```

Without `ShowComplete()`, you'd see just the problem: 487, × 36, a line, and nothing under it.

### 5b. Step through it with Advance

```csharp
public sealed class SteppedSubtractionSlide : AtriaSlide
{
    private SchoolbookArithmeticElement? _work;

    public SteppedSubtractionSlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size) { }

    // Show the step's sentence ("The ones column: 3 is less than 7, so borrow…") in the operator window.
    public override string CurrentBeat => _work?.CurrentStep?.Description ?? "Start";

    public override void Initialize()
    {
        _work = new SchoolbookArithmeticElement("#diff", SchoolbookWork.Subtraction(5003, 1867))
        {
            Size = new SSizeF(Size.Width * 0.5, Size.Height * 0.6)
        };
        var anchor = new BasisPoint(Center, "#diffAnchor");
        _work.AnchorCenterTo(anchor);
        Add([anchor, _work]);
    }

    public override SlideAdvanceResult Advance() =>
        _work!.StepForward() ? SlideAdvanceResult.InternalStateChanged : SlideAdvanceResult.CanAdvance;

    public override SlideAdvanceResult Rewind() =>
        _work!.StepBack() ? SlideAdvanceResult.InternalStateChanged : SlideAdvanceResult.CanRewind;
}
```

`StepForward()` returns `false` once the problem is finished, and `StepBack()` returns `false` when you're back at the start. That's why these one-liners work.

To show each step's sentence **on the slide**, put a `TextBlock` under the work and update it after each step. Use `textBlock.SetTextAndKeepFontSize(MeasurementService, _work.CurrentStep?.Description ?? "")`. The demo slide's `UpdateCaption` does exactly this.

### 5c. Auto-play with sound and the table

This is the "whole big number in one second" effect.

```csharp
public sealed class NoisyAutoPlaySlide : AtriaSlide
{
    private static readonly SchoolbookSound Sound = new();
    private SchoolbookArithmeticElement? _work;
    private OperationTableElement? _table;

    public NoisyAutoPlaySlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size) { }

    public override void Initialize()
    {
        var work = SchoolbookWork.Multiplication(
            BigInteger.Parse("31415926535897932"),
            BigInteger.Parse("27182818284590452"),
            layout: MultiplicationLayout.Folded);

        _work = new SchoolbookArithmeticElement("#big", work)
        {
            Size = new SSizeF(Size.Width * 0.6, Size.Height * 0.6)
        };
        var workAnchor = new BasisPoint(new SPointF(Size.Width * 0.36, Center.Y), "#bigAnchor");
        _work.AnchorCenterTo(workAnchor);

        _table = new OperationTableElement("#table")
        {
            Size = new SSizeF(Size.Height * 0.5, Size.Height * 0.5)
        };
        var tableAnchor = new BasisPoint(new SPointF(Size.Width * 0.81, Center.Y), "#tableAnchor");
        _table.AnchorCenterTo(tableAnchor);

        // Every step, whether from StepForward() or from auto-play, raises StepTaken.
        _work.StepTaken += (sender, step) =>
        {
            Sound.PlayStep(Runtime.Tones, step, _work.SecondsPerStep, _work.StepLateBySeconds);
            _table.Lookup = step.Lookups.Count > 0 ? step.Lookups[^1] : null;
        };

        Add([workAnchor, tableAnchor, _work, _table]);
    }

    public override SlideAdvanceResult Advance()
    {
        if (_work!.IsComplete) { return SlideAdvanceResult.CanAdvance; }
        if (_work.IsPlaying) { _work.ShowComplete(); }                 // second press: skip to the end
        else { _work.Play(1.0 / _work.Work.Steps.Count); }             // first press: whole thing in ~1 s
        return SlideAdvanceResult.InternalStateChanged;
    }
}
```

The `StepTaken` handler is the important part:

- **`Sound.PlayStep(...)`** plays the step's chords. Pass `_work.SecondsPerStep`, which is `null` when stepping by hand and the step length when auto-playing. That's how it picks between ringing notes and fast crossfaded ones. Pass `_work.StepLateBySeconds` so fast notes stay on the beat. Auto-play only gets to step on frames, so a step can be a few milliseconds late.
- **`_table.Lookup = ...`** lights up the last fact the step used.

`Play(secondsPerStep)` runs the steps on its own until the end, `Pause()` stops it, and `ShowComplete()` jumps to the end. Note that `ShowComplete()` does **not** raise `StepTaken`, so if you skip ahead you won't hear the remaining notes.

### 5d. A quick pop-up while you're doing other math

When the arithmetic is a side note, show the finished work small, fade it in, and take it away later:

```csharp
private SchoolbookArithmeticElement? _popUp;

public void ShowAside()
{
    _popUp = new SchoolbookArithmeticElement("#aside", SchoolbookWork.Addition(4738, 2594))
    {
        Size = new SSizeF(Size.Width * 0.25, Size.Height * 0.3)
    };
    _popUp.ShowComplete();
    var anchor = new BasisPoint(new SPointF(Size.Width * 0.85, Size.Height * 0.2), "#asideAnchor");
    _popUp.AnchorCenterTo(anchor);
    Add([anchor, _popUp]).AnimateBasic(0.4, AnimationTypes.FadeIn, Easings.Linear);
}

public void HideAside()
{
    if (_popUp != null) { Remove([_popUp]); _popUp = null; }
}
```

Call these from `Advance()` on whichever beat should show or hide it. For a big problem that would be too tall to read, use the **folded** multiplication or the **summary** division instead (see the next section).

---

## 6. Making the math: every option

All numbers are `BigInteger`, so any length works. Plain `int` and `long` values convert on their own. For numbers too big for `long`, use `BigInteger.Parse("...")`. Bases 2 through 16 work everywhere; digits above 9 show as A–F.

### Addition

```csharp
SchoolbookWork.Addition(4738, 2594);                          // two numbers
SchoolbookWork.Addition([99, 99, 99, 99]);                    // any number of addends (note the [ ])
SchoolbookWork.Addition(0b1011, 0b0111, numberBase: 2);       // binary: 1011 + 111
```

- No negative numbers.
- One step per column. Carries are written small above the next column.
- The last column writes its whole total. For example, "12" spills into a new column on the left.

### Subtraction

```csharp
SchoolbookWork.Subtraction(5003, 1867);
```

- The first number must be at least as big as the second, and neither may be negative. Otherwise it throws.
- Borrowing works across zeros, and a digit that gets rewritten twice stacks the second rewrite above the first. Borrowed-from digits are crossed out in red.
- A borrow takes its own step, then the subtraction for that column is another step.
- Leading zeros in the answer are left blank.

### Multiplication: three layouts

```csharp
SchoolbookWork.Multiplication(487, 36);                                            // DigitPairs (default)
SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.RowCarries);
SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.Folded);
```

- **`DigitPairs`** (default, the way you learned it): every bottom digit times every top digit gets its own row, shifted left with dim placeholder zeros. 487 × 36 has six rows: 42, 480, 2400, 210, 2400, 12000. Then they're all added up. That's n × m rows, so it gets tall fast with long numbers.
- **`RowCarries`**: one row per bottom digit, carrying within the row (the carries go above the top number), then the rows are added up.
- **`Folded`**: no partial rows at all, just the two numbers and the answer. Each step does one column of the answer by adding up every digit product that lands in that column, plus the carry. For example: "The tens column: 7×3 + 8×6 + 4 carried = 73. Write 3, carry 7." **This is the one for very long numbers.**

### Long division: two layouts

```csharp
SchoolbookWork.LongDivision(9876, 7);                                // 1410 R 6, worked step by step
SchoolbookWork.LongDivision(22, 7, decimalPlaces: 4);                // 3.1428, worked
SchoolbookWork.LongDivision(355, 113, decimalPlaces: 10,
    layout: DivisionLayout.Summary);                                 // 3.1415929203, answer only
```

- The divisor must be positive and the dividend not negative.
- **`decimalPlaces: 0`** (the default) stops at the whole number and writes `R` and the remainder in the quotient row. Any other value keeps going past a decimal point for that many digits. It stops early if the division comes out even, so 10 ÷ 4 to 3 places is just "2.5".
- **`Worked`** (default) does every step:
  1. **Shift:** "37 doesn't fit into 10, so start with 100."
  2. **The Barker term:** the biggest multiple that doesn't go over. "2 × 37 = 74 is the most that fits in 100. (3 × 37 = 111 would go over.) Write 2."
  3. **Write the product** under it with a minus sign.
  4. **Subtract** column by column. Borrows are explained in the sentence, not drawn as marks.
  5. **Bring down** the next digit, then repeat.

  When a partial is too small, it writes 0 in the quotient and brings down another digit. When the digits run out and decimals were asked for, it writes a decimal point and brings down zeros.
- **`Summary`** shows only the divisor, the bracket, the dividend and the quotient, with no steps.
- Worked division takes about two rows per quotient digit, so long ones get very small. For anything long, use `Summary`.

### Reading a work

You rarely need this, but you can look inside a `SchoolbookWork`:

- `work.Title`: e.g. `"487 × 36"`
- `work.Result`: the answer as a `BigInteger`. For division, it's the whole-number quotient.
- `work.Steps`: each step has:
  - `Description`, the sentence
  - `Lookups`, the table facts used (see section 8)
  - `Tone`
  - `Highlighted`, `Revealed`, `Hidden` and `StruckOut`, which are lists of cell numbers
- `work.Cells`: every piece of text, with its `Row`, `Column`, `Text` and `Kind`
- `work.GetState(stepIndex)`: which cells are visible, crossed out, highlighted and freshly written after a given step

You'd only use `Cells` and `GetState` if you wanted to draw the work some other way.

---

## 7. The elements, property by property

### `SchoolbookArithmeticElement`

`new SchoolbookArithmeticElement("#id", work)`, then set `Size`, anchor it, and add it.

**Where it is:**

| Member | What it does |
| --- | --- |
| `StepIndex` | Which step is showing. **−1 means nothing worked yet** (the default). `work.Steps.Count - 1` is finished. You can set it directly; it clamps to range. |
| `CurrentStep` | The step being shown, or `null` at −1. |
| `IsComplete` | True on the last step. A summary division has no steps, so it's always complete. |

**Moving it:**

| Member | What it does |
| --- | --- |
| `StepForward()` | One step on. Raises `StepTaken`. Returns `false` if already finished. |
| `StepBack()` | One step back, and stops any auto-play. Returns `false` at −1. Doesn't raise `StepTaken`. |
| `ShowComplete()` | Jumps to the end and stops auto-play. Doesn't raise `StepTaken`. |

**Auto-play:**

| Member | What it does |
| --- | --- |
| `Play(secondsPerStep)` | Steps on its own every `secondsPerStep` seconds until the end. For "the whole thing in N seconds", use `N / work.Steps.Count`. Very small values are fine; it takes several steps in one frame if it has to. |
| `Pause()` | Stops auto-play where it is. |
| `IsPlaying` | Whether auto-play is running. |
| `SecondsPerStep` | The auto-play step length, or `null` when not playing. |

**Sound hooks:**

| Member | What it does |
| --- | --- |
| `StepTaken` | Event raised with the `SchoolbookStep` every time a step is worked forward. This is where sound and table updates go. |
| `StepLateBySeconds` | During `StepTaken`, how late an auto-played step ran. Pass it to `SchoolbookSound.PlayStep`. |

**Looks:**

| Member | What it does |
| --- | --- |
| `FontFamily` | Default `"Cambria Math"`. If a Mac doesn't have it, Skia picks a fallback. |
| `SmallRowScale` | Height of carry and borrow rows compared to normal rows. Default 0.55. |
| `CellAspectRatio` | Digit cell width ÷ height. Default 0.7. |
| `TextColor` | Digits of the problem and answer. White. |
| `CarryColor` | Small carries and borrows. Orange. |
| `PlaceholderColor` | Shift zeros in multiplication. Dim lavender. |
| `WrittenColor` | Digits written in the current step. Yellow. |
| `HighlightColor` | Faint box behind every digit the current step reads or writes. Translucent white. |
| `StrikeColor` | Crossed-out digits. Red. |
| `RuleColor` | The lines under rows and the division bracket. White. |

A line under a row only appears once something in that row is visible. That's why the line under multiplication's partial products shows up with them.

### `OperationTableElement`

`new OperationTableElement("#id")` makes an addition table in base 10. You can also write `new OperationTableElement("#id", TableOperation.Multiplication, numberBase: 2)`.

| Member | What it does |
| --- | --- |
| `Lookup` | The fact to highlight (a `TableLookup`), or `null` for none. **Setting it also switches the table to that fact's operation and carry**, so one table can flip between + and × as the work goes. |
| `Operation` | `TableOperation.Addition` or `.Multiplication`. Set automatically by `Lookup`. |
| `Carry` | Shifts the addition columns: 0 is the plain table, 1 makes the columns 1–10 (a carried 1 is already added), and 14 makes them 14–23. Set automatically by `Lookup`. |
| `NumberBase` | Set in the constructor. The table is (base + 1) cells on each side, counting the header row and column. |
| Colors | `HeaderBackground`, `CellBackground`, `LineColor`, `HeaderTextColor`, `CellTextColor`, `CrossBackground` (the highlighted row and column), `HitBackground` and `HitTextColor` (the highlighted cell). |

It stays square inside whatever `Size` you give it. To hide it for a problem, don't add it, or `Remove` it.

---

## 8. Sound, from the top

### How sound gets out of the computer

1. A slide calls `Runtime.Tones.PlayTone(frequency, duration, …)`. `Runtime.Tones` is an `ITonePlayer`.
2. By default `Runtime.Tones` is `NullTonePlayer`, which ignores everything. **Slides never need to check whether sound works.** They just call it.
3. The **harness on Windows** replaces it with `NAudioTonePlayer` when a presentation starts (in `PresentationController`'s constructor), so tones come out of the default speakers. If no audio device can be opened, it quietly stays silent.
4. **On macOS there's no real player yet**, so everything is silent. Adding one means writing a class that implements `ITonePlayer` and setting it in the same place in `PresentationController`. `ToneSynth.RenderSine` already makes the samples, so a Mac player only has to send a float array to the speakers.

### What the schoolbook steps sound like

Each step lists the **table lookups** it does. Each lookup is one cell of the addition or times table: column **X**, row **Y**, and, for additions, a **carry** of any size.

`SchoolbookSound.PlayStep` plays each lookup as **two notes at once**:

- **Column note = X + carry**, starting at middle C. So a plain 3 + 7 plays note 3 for the column, and 3 + 7 with 4 carried plays note 7.
- **Row note = 12 + Y**, one octave higher. So Y = 7 plays note 19.

A step with several lookups plays its chords one after another, spread across the step.

**Note numbers:** middle C (C4) is **0**, every +1 is one semitone, +12 is an octave up, and negative numbers go below middle C. **A (note 9) is 440 Hz.** `NotePitch.Frequency(n)` turns a note number into hertz. These are plain sine waves, not piano sounds.

Which lookups each operation records:

| Operation | Lookups |
| --- | --- |
| Addition column | One lookup per addition. First digit + second digit + the carry coming in. With more digits (or many rows), each further digit is added to the running total, split into its last digit (X) and the rest (the carry), so X + carry is always the total so far. |
| Subtraction | The addition table read backwards: 13 − 7 = 6 plays 7 + 6. |
| Multiplication | One times-table lookup per digit product. Folded columns then add up the products' last digits with the carry. |
| Long division | The Barker term plays the divisor's digits times the quotient digit. Each subtraction column plays its addition-backwards lookup, with any borrow as the carry. |

Borrowing steps and placeholder zeros have no lookups, so they're silent.

**Slow versus fast:**

- **Stepping by hand, or slow auto-play:** each note has a quick attack, then rings and dies away in about a tenth of a second.
- **Fast auto-play** (steps shorter than 0.15 s): notes don't ring. Each one lasts two slots and **crossfades** into the next, so there are no clicks or pops even when hundreds of notes go by in a second.

**Very high notes:** a long column's running total can get big, so any note above C7 (note 36) drops down by octaves until it fits.

### Knobs on `SchoolbookSound`

Set these when you create it, e.g. `new SchoolbookSound { Volume = 0.1f }`.

| Property | Default | What it does |
| --- | --- | --- |
| `ColumnBaseNote` | 0 (middle C) | Note for column 0. |
| `RowBaseNote` | 12 | Note for row 0. |
| `MaximumNote` | 36 | Higher notes drop by octaves. |
| `Volume` | 0.15 | Per note. A chord is two notes. |
| `RingSeconds` | 0.5 | How long a slow note lasts. |
| `DecaySeconds` | 0.1 | How quickly a slow note dies away. |
| `ChordSpacingSeconds` | 0.09 | Most time between chords within one slow step. |
| `FastStepSeconds` | 0.15 | Steps shorter than this use crossfaded fast notes. |
| `FastLeadSeconds` | 0.025 | A tiny delay on fast notes so late steps can still land on the beat. |

### Playing your own tones

```csharp
Runtime.Tones.PlayTone(NotePitch.Frequency(9), 0.5, 0.2f, shape: ToneShape.Ringing(0.1));  // an A that rings
```

`PlayTone(frequencyHz, durationSeconds, volume, delaySeconds, shape)`:

- `delaySeconds` schedules the tone a little in the future.
- `shape` is a `ToneShape`:
  - `ToneShape.Default`: a plain beep with 5 ms fades
  - `ToneShape.Ringing(decay)`: an attack, then it dies away
  - `ToneShape.Crossfade(seconds)`: smooth fades for back-to-back notes

`PitchMapper` is also there if you ever want sorting-video-style "value 0…N → 120…1200 Hz" mapping instead of notes.

---

## 9. Gotchas

- **"Nothing happens when I double-click a slide."** Press **Start** first (section 3).
- **"My text is invisible."** `TextBlock` starts at `Opacity = 0`. Set `Opacity = 1` or fade it in.
- **"It's tiny."** The element scales to fit its `Size`. Tall layouts shrink: long digit-pair multiplication, or worked division with many digits. Use `Folded` or `Summary`, or give it a taller `Size`.
- **"No sound."** On macOS that's expected for now. On Windows, check that the default output device works. Sound only plays for steps that have lookups, and only when something calls `SchoolbookSound.PlayStep` (or `PlayTone`) from your `StepTaken` handler.
- **"ShowComplete skipped the notes."** It doesn't raise `StepTaken`. Use `Play(...)` if you want to hear it.
- **"Subtraction threw an exception."** The first number has to be at least as big as the second.
- **"I changed the work and nothing updated."** A `SchoolbookWork` is fixed once made. Make a new one and a new element: `Remove` the old element and `Add` the new one. The demo slide's `ShowProblem` does this.

---

## 10. Where to look next

- **The demo slide**, `Celarix.Starfall.Decks/Playground/MathFun/SchoolbookArithmeticSlide.cs`, shows a list of problems with labels, moving between them with Advance and Rewind, a caption, the table, sound, and per-problem auto-play.
- **The tests**, `Celarix.Starfall.Tests/Mathematics/SchoolbookWorkTests.cs`, have short examples of every operation and what each one produces: partial-product rows, lookups, notes.
- **The older addition table** in `Celarix.Starfall.Decks/Playground/AtriaTests/AdditionTableElement.cs` (the "Math Fun" slide) is what `OperationTableElement` was modeled on.
