using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Layout.Atria.Elements;

/// <summary>
/// Displays a titled month as a borderless Monday-first calendar grid.
/// </summary>
public sealed class MonthCalendar : AtriaElement
{
    private const int DaysPerWeek = 7;
    private static readonly string[] WeekdayHeadings = ["M", "T", "W", "T", "F", "S", "S"];

    private readonly SColor[] dayColors;
    private readonly AnimationSlot?[] colorAnimationSlots;
    private readonly int firstDayOffset;
    private readonly int weekRowCount;

    public string Title { get; }
    public DayOfWeek FirstDayOfWeek { get; }
    public int DaysInMonth { get; }
    public SFont Font { get; }
    public SColor DefaultFontColor { get; }

    public MonthCalendar(
        string title,
        DayOfWeek firstDayOfWeek,
        int daysInMonth,
        SFont font,
        SColor defaultFontColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(font);
        if (daysInMonth is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(daysInMonth), "A month must contain 1 through 31 days.");
        }

        Title = title;
        FirstDayOfWeek = firstDayOfWeek;
        DaysInMonth = daysInMonth;
        Font = font;
        DefaultFontColor = defaultFontColor;

        firstDayOffset = ((int)firstDayOfWeek - (int)DayOfWeek.Monday + DaysPerWeek) % DaysPerWeek;
        weekRowCount = (int)Math.Ceiling((firstDayOffset + daysInMonth) / (double)DaysPerWeek);
        dayColors = Enumerable.Repeat(defaultFontColor, daysInMonth).ToArray();
        colorAnimationSlots = new AnimationSlot?[daysInMonth];

        var fontSize = font.Size ?? 24f;
        Size = new SSizeF(fontSize * 14d, fontSize * (2.1d + (weekRowCount * 1.6d)));
        Opacity = 1d;
    }

    protected override void OnAttached()
    {
        for (var dayIndex = 0; dayIndex < colorAnimationSlots.Length; dayIndex++)
        {
            colorAnimationSlots[dayIndex] = Animations.CreateSlot($"MonthCalendar.Day{dayIndex + 1}.Color");
        }
    }

    public SColor GetColor(int dayNumber) => dayColors[DayIndex(dayNumber)];

    public void SetColor(int dayNumber, SColor color)
    {
        var dayIndex = DayIndex(dayNumber);
        colorAnimationSlots[dayIndex]?.FinishNow();
        dayColors[dayIndex] = color;
    }

    public void AnimateSetColor(int dayNumber, SColor color, double duration, Easing easing)
    {
        ArgumentNullException.ThrowIfNull(easing);
        if (duration < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative.");
        }

        var dayIndex = DayIndex(dayNumber);
        var slot = colorAnimationSlots[dayIndex]
            ?? throw new InvalidOperationException("The calendar must be added to a slide before animating its colors.");
        slot.FinishNow();

        var from = dayColors[dayIndex];
        var durationFrames = Math.Max(1, AnimationContext.SecondsToFrames(duration));
        slot.Replace(Animations.StartNow(durationFrames, progress =>
        {
            dayColors[dayIndex] = Interpolators.Get<SColor>().Interpolate(
                from,
                color,
                easing(progress));
        }));
    }

    /// <summary>
    /// Gets the bounds of the centered text cell used to draw a day number.
    /// The returned bounds are in slide coordinates and track this element's position and size.
    /// </summary>
    public SRectF GetDayTextBounds(int dayNumber)
    {
        DayIndex(dayNumber);
        var (gridTop, rowHeight, columnWidth) = GetGridLayout();
        var cellIndex = firstDayOffset + dayNumber - 1;
        var column = cellIndex % DaysPerWeek;
        var row = (cellIndex / DaysPerWeek) + 1;
        return CellBounds(column, row, gridTop, columnWidth, rowHeight);
    }

    public override void Render(IRenderTarget target)
    {
        var titleHeight = Size.Height / (weekRowCount + 2d);
        var (gridTop, rowHeight, columnWidth) = GetGridLayout();
        var visibleDefaultColor = DefaultFontColor.WithOpacity(Opacity);

        target.DrawText(
            Title,
            Font,
            new SRectF(Bounds.Left, Bounds.Top, Size.Width, titleHeight),
            visibleDefaultColor,
            SAngle.Zero,
            Alignment.Center);

        for (var column = 0; column < DaysPerWeek; column++)
        {
            target.DrawText(
                WeekdayHeadings[column],
                Font,
                CellBounds(column, 0, gridTop, columnWidth, rowHeight),
                visibleDefaultColor,
                SAngle.Zero,
                Alignment.Center);
        }

        for (var dayNumber = 1; dayNumber <= DaysInMonth; dayNumber++)
        {
            target.DrawText(
                dayNumber.ToString(),
                Font,
                GetDayTextBounds(dayNumber),
                GetColor(dayNumber).WithOpacity(Opacity),
                SAngle.Zero,
                Alignment.Center);
        }
    }

    private (double GridTop, double RowHeight, double ColumnWidth) GetGridLayout()
    {
        var titleHeight = Size.Height / (weekRowCount + 2d);
        return (
            Bounds.Top + titleHeight,
            (Size.Height - titleHeight) / (weekRowCount + 1d),
            Size.Width / DaysPerWeek);
    }

    private SRectF CellBounds(int column, int row, double gridTop, double columnWidth, double rowHeight) =>
        new(
            Bounds.Left + (column * columnWidth),
            gridTop + (row * rowHeight),
            columnWidth,
            rowHeight);

    private int DayIndex(int dayNumber)
    {
        if (dayNumber < 1 || dayNumber > DaysInMonth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dayNumber),
                $"Day number must be between 1 and {DaysInMonth}.");
        }

        return dayNumber - 1;
    }
}
