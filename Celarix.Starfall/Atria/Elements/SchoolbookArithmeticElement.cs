using Celarix.Starfall.Mathematics.Schoolbook;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Atria.Elements;

/// <summary>
/// Draws a problem worked out by hand (long addition, subtraction or multiplication) and steps
/// through it one column at a time, highlighting the digits each step reads and writes.
/// </summary>
/// <remarks>
/// The grid is scaled to fit <see cref="AtriaElement.Size"/> and centered in it, so long problems
/// simply get smaller. Drive it by hand with <see cref="StepIndex"/>, <see cref="StepForward"/> and
/// <see cref="StepBack"/>; let it run on its own with <see cref="Play"/>; or show the finished
/// work at once with <see cref="ShowComplete"/>.
/// </remarks>
public sealed class SchoolbookArithmeticElement : AtriaElement
{
    private int _stepIndex = -1;
    private SchoolbookState _state;
    private double? _secondsPerStep;
    private double _secondsUntilStep;
    private double _stepLateBySeconds;

    public SchoolbookArithmeticElement(string atriaIdString, SchoolbookWork work)
    {
        Id = AtriaId.Parse(atriaIdString);
        Work = work;
        Opacity = 1d;
        _state = work.GetState(-1);
    }

    public SchoolbookWork Work { get; }

    /// <summary>
    /// Gets or sets how many steps have been worked, counting from 0. −1 (the default) shows only the problem.
    /// </summary>
    public int StepIndex
    {
        get => _stepIndex;
        set
        {
            var clamped = Math.Clamp(value, -1, Work.Steps.Count - 1);
            if (clamped == _stepIndex) { return; }
            _stepIndex = clamped;
            _state = Work.GetState(clamped);
        }
    }

    /// <summary>Gets the step shown now, or null before the first step.</summary>
    public SchoolbookStep? CurrentStep => _stepIndex >= 0 ? Work.Steps[_stepIndex] : null;

    public bool IsComplete => _stepIndex == Work.Steps.Count - 1;

    /// <summary>Gets whether <see cref="Play"/> is stepping through the work.</summary>
    public bool IsPlaying => _secondsPerStep != null;

    /// <summary>Gets the time between steps while <see cref="Play"/> runs, or null when not playing.</summary>
    public double? SecondsPerStep => _secondsPerStep;

    /// <summary>
    /// Raised whenever <see cref="StepForward"/> or <see cref="Play"/> works a step, e.g. to play its tone.
    /// </summary>
    public event EventHandler<SchoolbookStep>? StepTaken;

    /// <summary>
    /// Gets, during <see cref="StepTaken"/>, how long ago the step was due. Auto-play only runs on
    /// frames, so a step can land a few milliseconds late; sound can use this to stay on the beat.
    /// </summary>
    public double StepLateBySeconds => _stepLateBySeconds;

    public string FontFamily { get; set; } = "Cambria Math";

    /// <summary>Gets or sets the height of carry and borrow rows, as a fraction of a normal row.</summary>
    public double SmallRowScale { get; set; } = 0.55d;

    /// <summary>Gets or sets a cell's width as a fraction of its height.</summary>
    public double CellAspectRatio { get; set; } = 0.7d;

    public SColor TextColor { get; set; } = SColor.White;
    public SColor CarryColor { get; set; } = new(255, 170, 80, 255);
    public SColor PlaceholderColor { get; set; } = new(150, 150, 190, 255);
    public SColor WrittenColor { get; set; } = new(255, 230, 90, 255);
    public SColor HighlightColor { get; set; } = new(255, 255, 255, 50);
    public SColor StrikeColor { get; set; } = new(255, 90, 90, 255);
    public SColor RuleColor { get; set; } = SColor.White;

    /// <summary>Moves to the next step. Returns false if the problem is already finished.</summary>
    public bool StepForward()
    {
        if (IsComplete) { return false; }
        StepIndex += 1;
        StepTaken?.Invoke(this, Work.Steps[_stepIndex]);
        _stepLateBySeconds = 0d;
        return true;
    }

    /// <summary>
    /// Works the remaining steps on its own, one every <paramref name="secondsPerStep"/> seconds,
    /// raising <see cref="StepTaken"/> for each. Stops when the work is complete.
    /// </summary>
    public void Play(double secondsPerStep)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(secondsPerStep);
        _secondsPerStep = secondsPerStep;
        _secondsUntilStep = 0d;
    }

    public void Pause() => _secondsPerStep = null;

    /// <summary>Jumps straight to the finished work, without raising <see cref="StepTaken"/>.</summary>
    public void ShowComplete()
    {
        Pause();
        StepIndex = Work.Steps.Count - 1;
    }

    public override void Update(FrameTime frameTime)
    {
        if (_secondsPerStep is not { } secondsPerStep) { return; }

        _secondsUntilStep -= frameTime.Delta.TotalSeconds;
        while (_secondsUntilStep <= 0d)
        {
            _stepLateBySeconds = -_secondsUntilStep;
            if (!StepForward())
            {
                Pause();
                return;
            }
            _secondsUntilStep += secondsPerStep;
        }
    }

    /// <summary>Moves to the previous step. Returns false if no steps have been worked.</summary>
    public bool StepBack()
    {
        Pause();
        if (_stepIndex < 0) { return false; }
        StepIndex -= 1;
        return true;
    }

    public override void Render(IRenderTarget target)
    {
        if (Size.Width <= 0 || Size.Height <= 0 || Opacity <= 0) { return; }

        var rowScales = Work.Rows.Select(r => r == SchoolbookRowKind.Small ? SmallRowScale : 1d).ToArray();
        var heightUnits = rowScales.Sum();
        var cellHeight = Math.Min(Size.Height / heightUnits, Size.Width / (Work.ColumnCount * CellAspectRatio));
        var cellWidth = cellHeight * CellAspectRatio;
        var left = Position.X + (Size.Width - cellWidth * Work.ColumnCount) / 2;
        var top = Position.Y + (Size.Height - cellHeight * heightUnits) / 2;

        var rowTops = new double[rowScales.Length + 1];
        rowTops[0] = top;
        for (var r = 0; r < rowScales.Length; r++) { rowTops[r + 1] = rowTops[r] + rowScales[r] * cellHeight; }

        var font = new SFontFamily(FontFamily, null);
        var normalSize = target.FitTextToHeight("0", font, (float)(cellHeight * 0.8));
        var smallSize = target.FitTextToHeight("0", font, (float)(cellHeight * SmallRowScale * 0.85));
        var normalFont = font.WithSize(normalSize);
        var smallFont = font.WithSize(smallSize);

        SRectF CellRect(SchoolbookCell cell) => new(left + cell.Column * cellWidth, rowTops[cell.Row],
            cellWidth, rowTops[cell.Row + 1] - rowTops[cell.Row]);

        // Highlights sit behind everything else.
        foreach (var index in _state.Highlighted.Concat(_state.Written))
        {
            if (!_state.Visible[index]) { continue; }
            var rect = CellRect(Work.Cells[index]);
            target.DrawRectangle(Inset(rect, cellWidth * 0.06), Fade(HighlightColor), SPaintStyle.Fill, SAngle.Zero);
        }

        var ruleThickness = (float)Math.Max(1d, cellHeight * 0.05);
        foreach (var rule in Work.Rules)
        {
            var y = rowTops[rule.AfterRow + 1] + cellHeight * 0.05;
            target.DrawLine(new SPointF(left + rule.FirstColumn * cellWidth, y),
                new SPointF(left + (rule.LastColumn + 1) * cellWidth, y), Fade(RuleColor), ruleThickness);
        }

        for (var i = 0; i < Work.Cells.Count; i++)
        {
            if (!_state.Visible[i]) { continue; }
            var cell = Work.Cells[i];
            var rect = CellRect(cell);
            var small = Work.Rows[cell.Row] == SchoolbookRowKind.Small;
            target.DrawText(cell.Text, small ? smallFont : normalFont, rect, Fade(ColorFor(cell, i)), SAngle.Zero);

            if (_state.StruckOut[i])
            {
                var strike = Inset(rect, cellWidth * 0.12);
                target.DrawLine(new SPointF(strike.Left, strike.Bottom), new SPointF(strike.Right, strike.Top),
                    Fade(StrikeColor), (float)Math.Max(1d, cellHeight * 0.04));
            }
        }
    }

    private SColor ColorFor(SchoolbookCell cell, int index)
    {
        if (_state.Written.Contains(index)) { return WrittenColor; }
        return cell.Kind switch
        {
            SchoolbookCellKind.Carry or SchoolbookCellKind.Borrow => CarryColor,
            SchoolbookCellKind.Placeholder => PlaceholderColor,
            _ => TextColor
        };
    }

    private SColor Fade(SColor color) => new(color.R, color.G, color.B, (byte)(color.A * Math.Clamp(Opacity, 0d, 1d)));

    private static SRectF Inset(SRectF rect, double amount) =>
        new(rect.X + amount, rect.Y + amount, Math.Max(0, rect.Width - 2 * amount), Math.Max(0, rect.Height - 2 * amount));
}
