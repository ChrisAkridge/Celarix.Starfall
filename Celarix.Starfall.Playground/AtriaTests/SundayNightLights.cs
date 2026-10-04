using Celarix.Starfall.Atria;
using Celarix.Starfall.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using Celarix.Starfall.Rendering.Targets;
using System.Globalization;

namespace Celarix.Starfall.Playground.AtriaTests;

internal sealed class SundayNightLights : AtriaSlide
{
    private const int DateCount = 365;
    private const int ThanksgivingDateIndex = 332; // November 29 in a non-leap year.
    private const int ThirdThursdayDateIndex = 325; // November 22 in a non-leap year.
    private const int SecondThursdayDateIndex = 318; // November 15 in a non-leap year.
    private const int FirstThursdayDateIndex = 311; // November 8 in a non-leap year.
    private const int NovemberFirstDateIndex = 304;
    private const double ArrowTargetX = 400d;
    private const double ArrowWidth = 25d;
    private const double ListLeftMargin = 18d;
    private const double DateRowHeight = 36d;
    private const double DateTopMargin = 8d;
    private const double DateLabelHeight = DateRowHeight - DateTopMargin;
    private const double DateColumnWidth = 170d;
    private const double DefaultBracketWidth = 22d;
    private const double SecondaryLabelMargin = 14d;
    private const double SecondaryLabelFadeDurationSeconds = 0.5d;
    private const double BeatSevenDurationSeconds = 0.8d;
    private const double BeatSevenShiftX = -300d;
    private const double BeatEightDurationSeconds = 1.2d;
    private const double BeatEightOrdinalEntranceDurationSeconds = 0.35d;
    private const double BeatEightOrdinalEntranceDistance = 28d;
    private const double OrdinalColumnGap = 20d;
    private const double BeatTenFadeDurationSeconds = 0.4d;
    private const double BeatElevenCellFadeDurationSeconds = 0.3d;
    private const double BeatElevenStaggerDelaySeconds = 0.16d;
    private const double CalendarHighlightBorderWidth = 2d;
    private const double BeatTwelveDurationSeconds = 1d;
    private const double BeatTwelveContentExitX = -1400d;
    private const double CalendarGridWidth = 260d;
    private const double CalendarGridHeight = 250d;
    private const double CalendarGridHorizontalGap = 25d;
    private const double CalendarGridTopY = 40d;
    private const double CalendarGridBottomY = 390d;
    private const double BeatThirteenDurationSeconds = 0.8d;

    private sealed class BracketedRange(int firstDateIndex, int lastDateIndex, double width)
    {
        public int FirstDateIndex { get; } = firstDateIndex;
        public int LastDateIndex { get; } = lastDateIndex;
        public double Width { get; } = width;
        public double Top { get; set; }
        public double Bottom { get; set; }
        public bool IsSettled { get; set; }
    }

    private static readonly SFont DateFont = new SFontFamily("Cambria Math", 24f);
    private static readonly SColor HighlightedDateColor = new(184, 150, 20, 255);
    private static readonly SColor OrdinalColor = HighlightedDateColor.WithHue(0d);
    private static readonly SColor ReverseOrdinalColor = HighlightedDateColor.WithHue(120d);
    private static readonly SColor CalendarRedFill = SColor.FromHSV(0d, 0.42d, 0.62d);
    private static readonly SColor CalendarGreenFill = SColor.FromHSV(120d, 0.42d, 0.62d);
    private static readonly string[] DateLabels = BuildDateLabels();
    private static readonly Easing DateListEasing =
        Easings.AccelerateCruiseDecelerate(0.15d, 0.45d);
    private static readonly SPathStyle BracketStyle = new(
        null,
        SColor.White,
        2d,
        SStrokeCap.Butt,
        SStrokeJoin.Miter);

    private double listAnimationElapsedSeconds;
    private double listTop;
    private double rangeBeatElapsedSeconds;
    private double rangeBeatStartingListTop;
    private double beatSevenElapsedSeconds;
    private double beatEightElapsedSeconds;
    private double beatNineElapsedSeconds;
    private double beatTenElapsedSeconds;
    private double beatElevenElapsedSeconds;
    private double beatTwelveElapsedSeconds;
    private double beatThirteenElapsedSeconds;
    private double contentHorizontalOffset;
    private double beatTwelveContentStartX;
    private SPointF beatSevenArrowStart;
    private SPointF beatTwelveArrowStart;
    private SRectF beatTwelveCalendarStartBounds;
    private readonly SRectF[] beatThirteenCalendarStartBounds = new SRectF[7];
    private SlidingListArrowElement? arrow;
    private MonthCalendar? monthCalendar;
    private readonly MonthCalendar?[] weekdayCalendars = new MonthCalendar?[7];
    private BracketedRange? activeBracket;
    private int rangeBeatFirstDateIndex;
    private int rangeBeatLastDateIndex;
    private string? rangeBeatFirstDateLabel;
    private int currentBeat;

    private readonly List<BracketedRange> bracketedRanges = [];
    private readonly string?[] secondaryLabels = new string?[DateCount];
    private readonly double[] secondaryLabelFadeElapsedSeconds = new double[DateCount];
    private readonly double[] secondaryLabelOpacities = new double[DateCount];
    private readonly bool[] secondaryLabelsFading = new bool[DateCount];

    public bool[] HighlightedDates { get; } = new bool[DateCount];

    public SundayNightLights(AtriaRuntime runtime, SSizeF size) : base(runtime, size)
    {
    }

    public override void Initialize()
    {
        BackgroundColor = SColor.StarfallDefault;
        listTop = InitialListTop;

        arrow = new SlidingListArrowElement(targetX: ArrowTargetX, width: ArrowWidth);
        Add([arrow]);
    }

    public override void Update(FrameTime frameTime)
    {
        base.Update(frameTime);

        if (currentBeat is 1 or 2)
        {
            listAnimationElapsedSeconds += frameTime.Delta.TotalSeconds;
            var progress = Math.Clamp(
                listAnimationElapsedSeconds / SlidingListArrowElement.ArrivalDurationSeconds,
                0d,
                1d);
            listTop = MathHelpers.Ease(
                InitialListTop,
                ListTopForCenteredDate(ThanksgivingDateIndex),
                progress,
                DateListEasing);
        }

        if (currentBeat >= 3)
        {
            UpdateRangeBeat(frameTime.Delta.TotalSeconds);
        }

        if (currentBeat >= 7)
        {
            UpdateBeatSeven(frameTime.Delta.TotalSeconds);
        }
        if (currentBeat >= 8)
        {
            beatEightElapsedSeconds += frameTime.Delta.TotalSeconds;
        }
        if (currentBeat >= 9)
        {
            beatNineElapsedSeconds += frameTime.Delta.TotalSeconds;
        }
        if (currentBeat >= 10)
        {
            beatTenElapsedSeconds += frameTime.Delta.TotalSeconds;
        }
        if (currentBeat >= 11)
        {
            beatElevenElapsedSeconds += frameTime.Delta.TotalSeconds;
        }
        if (currentBeat >= 12)
        {
            UpdateBeatTwelve(frameTime.Delta.TotalSeconds);
        }
        if (currentBeat >= 13)
        {
            UpdateBeatThirteen(frameTime.Delta.TotalSeconds);
        }

        UpdateSecondaryLabelFades(frameTime.Delta.TotalSeconds);
    }

    public override SlideAdvanceResult Advance()
    {
        if (currentBeat == 0)
        {
            currentBeat = 1;
            listAnimationElapsedSeconds = 0d;
            arrow!.StartAnimation();
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 1)
        {
            currentBeat = 2;
            StartSecondaryLabelFade(
                ThanksgivingDateIndex,
                "Thanksgiving ≝ fourth Thursday of November");
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 2)
        {
            StartRangeBeat(
                beat: 3,
                ThirdThursdayDateIndex,
                ThanksgivingDateIndex,
                "third Thursday of November");
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 3)
        {
            StartRangeBeat(
                beat: 4,
                SecondThursdayDateIndex,
                ThirdThursdayDateIndex,
                "second Thursday of November");
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 4)
        {
            StartRangeBeat(
                beat: 5,
                FirstThursdayDateIndex,
                SecondThursdayDateIndex,
                "first Thursday of November");
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 5)
        {
            StartRangeBeat(
                beat: 6,
                NovemberFirstDateIndex,
                FirstThursdayDateIndex,
                "last Thursday of... October?");
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 6)
        {
            currentBeat = 7;
            beatSevenElapsedSeconds = 0d;
            beatSevenArrowStart = arrow!.Position;
            monthCalendar = new MonthCalendar(
                "November",
                DayOfWeek.Thursday,
                30,
                DateFont,
                SColor.White)
            {
                Position = new SPointF(890d, 205d),
                Size = new SSizeF(340d, 310d),
                Opacity = 0d
            };
            Add([monthCalendar]);
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 7)
        {
            currentBeat = 8;
            beatEightElapsedSeconds = 0d;
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 8)
        {
            currentBeat = 9;
            beatNineElapsedSeconds = 0d;
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 9)
        {
            currentBeat = 10;
            beatTenElapsedSeconds = 0d;
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 10)
        {
            currentBeat = 11;
            beatElevenElapsedSeconds = 0d;
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 11)
        {
            StartBeatTwelve();
            return SlideAdvanceResult.InternalStateChanged;
        }

        if (currentBeat == 12)
        {
            StartBeatThirteen();
            return SlideAdvanceResult.InternalStateChanged;
        }

        return SlideAdvanceResult.CanAdvance;
    }

    public override void KeyUp(SKeyboardEvent keyboardEvent)
    {
        if (keyboardEvent.Key == SKey.Right)
        {
            Advance();
        }
    }

    public override void Render(IRenderTarget target)
    {
        target.Clear(BackgroundColor);
        foreach (var element in Elements)
        {
            if (element is MonthCalendar calendar)
            {
                DrawCalendarHighlights(target, calendar);
            }
            element.Render(target);
        }

        if (DebugMode.ShowAnchors)
        {
            foreach (var basisElement in BasisElements)
            {
                basisElement.RenderDebug(target);
            }
        }

        var firstVisibleIndex = Math.Max(0, (int)Math.Floor(-listTop / DateRowHeight));
        var lastVisibleIndex = Math.Min(
            DateCount - 1,
            (int)Math.Ceiling((Size.Height - listTop) / DateRowHeight));

        for (var dateIndex = firstVisibleIndex; dateIndex <= lastVisibleIndex; dateIndex++)
        {
            var labelBounds = GetDateLabelBounds(dateIndex);
            var color = HighlightedDates[dateIndex] ? HighlightedDateColor : SColor.White;

            target.DrawText(
                DateLabels[dateIndex],
                DateFont,
                labelBounds,
                color,
                SAngle.Zero,
                Alignment.LeftCenter);
        }

        DrawBracketedRanges(target);
        DrawSecondaryLabels(target, firstVisibleIndex, lastVisibleIndex);
        DrawBeatEightOrdinals(target);
        DrawBeatNineOrdinals(target);
    }

    private void DrawCalendarHighlights(IRenderTarget target, MonthCalendar calendar)
    {
        if (currentBeat < 10 || monthCalendar is null) { return; }

        if (currentBeat >= 12)
        {
            DrawBeatTwelveCalendarHighlights(target, calendar);
            return;
        }

        if (!ReferenceEquals(calendar, monthCalendar)) { return; }

        if (currentBeat == 10)
        {
            var opacity = Easings.Smoothstep(Math.Clamp(
                beatTenElapsedSeconds / BeatTenFadeDurationSeconds,
                0d,
                1d));
            DrawCalendarHighlightCell(
                target, calendar, dayNumber: 22, opacity, ReverseOrdinalColor, CalendarGreenFill);
            DrawCalendarHighlightCell(
                target, calendar, dayNumber: 29, opacity, OrdinalColor, CalendarRedFill);
            return;
        }

        DrawCalendarHighlightCell(
            target, calendar, dayNumber: 22, 1d, ReverseOrdinalColor, CalendarGreenFill);
        DrawCalendarHighlightCell(
            target, calendar, dayNumber: 29, 1d, OrdinalColor, CalendarRedFill);

        DrawStaggeredCalendarHighlight(target, 23, stage: 0, ReverseOrdinalColor, CalendarGreenFill);
        DrawStaggeredCalendarHighlight(target, 24, stage: 1, ReverseOrdinalColor, CalendarGreenFill);
        DrawStaggeredCalendarHighlight(target, 25, stage: 2, ReverseOrdinalColor, CalendarGreenFill);
        DrawStaggeredCalendarHighlight(target, 26, stage: 3, ReverseOrdinalColor, CalendarGreenFill);
        DrawStaggeredCalendarHighlight(target, 27, stage: 4, ReverseOrdinalColor, CalendarGreenFill);
        DrawStaggeredCalendarHighlight(target, 28, stage: 5, ReverseOrdinalColor, CalendarGreenFill);

        DrawStaggeredCalendarHighlight(target, 30, stage: 0, OrdinalColor, CalendarRedFill);
    }

    private void DrawBeatTwelveCalendarHighlights(
        IRenderTarget target,
        MonthCalendar calendar)
    {
        var progress = Easings.Smoothstep(Math.Clamp(
            beatTwelveElapsedSeconds / BeatTwelveDurationSeconds,
            0d,
            1d));

        if (ReferenceEquals(calendar, monthCalendar))
        {
            var departingOpacity = 1d - progress;
            DrawFinalBeatElevenHighlights(target, calendar, departingOpacity);
            DrawCalendarHighlightCell(
                target,
                calendar,
                dayNumber: 22,
                opacity: 1d,
                ReverseOrdinalColor,
                CalendarGreenFill);
            return;
        }

        var thanksgivingDay = ThanksgivingDayFor(calendar.FirstDayOfWeek);
        DrawCalendarHighlightCell(
            target,
            calendar,
            thanksgivingDay,
            calendar.Opacity,
            ReverseOrdinalColor,
            CalendarGreenFill);
    }

    private void DrawFinalBeatElevenHighlights(
        IRenderTarget target,
        MonthCalendar calendar,
        double opacity)
    {
        for (var dayNumber = 22; dayNumber <= 28; dayNumber++)
        {
            if (dayNumber == 22) { continue; }
            DrawCalendarHighlightCell(
                target, calendar, dayNumber, opacity, ReverseOrdinalColor, CalendarGreenFill);
        }
        for (var dayNumber = 29; dayNumber <= 30; dayNumber++)
        {
            DrawCalendarHighlightCell(
                target, calendar, dayNumber, opacity, OrdinalColor, CalendarRedFill);
        }
    }

    private void DrawStaggeredCalendarHighlight(
        IRenderTarget target,
        int dayNumber,
        int stage,
        SColor borderColor,
        SColor fillColor)
    {
        var delay = stage * BeatElevenStaggerDelaySeconds;
        var progress = Easings.Smoothstep(Math.Clamp(
            (beatElevenElapsedSeconds - delay) / BeatElevenCellFadeDurationSeconds,
            0d,
            1d));
        DrawCalendarHighlightCell(
            target, monthCalendar!, dayNumber, progress, borderColor, fillColor);
    }

    private void DrawCalendarHighlightCell(
        IRenderTarget target,
        MonthCalendar calendar,
        int dayNumber,
        double opacity,
        SColor borderColor,
        SColor fillColor)
    {
        if (opacity <= 0d) { return; }

        var cellBounds = calendar.GetDayTextBounds(dayNumber);
        target.DrawRectangle(
            cellBounds,
            borderColor.WithOpacity(opacity),
            SPaintStyle.Fill,
            SAngle.Zero);

        var innerBounds = cellBounds.Shrink(
            CalendarHighlightBorderWidth,
            CalendarHighlightBorderWidth);
        target.DrawRectangle(
            innerBounds,
            fillColor.WithOpacity(opacity),
            SPaintStyle.Fill,
            SAngle.Zero);
    }

    private SRectF GetDateLabelBounds(int dateIndex)
    {
        var rowTop = listTop + (dateIndex * DateRowHeight);
        return new SRectF(
            ArrowTargetX + ListLeftMargin + contentHorizontalOffset,
            rowTop + DateTopMargin,
            DateColumnWidth,
            DateLabelHeight);
    }

    private void DrawBracketedRanges(IRenderTarget target)
    {
        var bracketLeft = ArrowTargetX + ListLeftMargin + DateColumnWidth + contentHorizontalOffset;
        foreach (var range in bracketedRanges)
        {
            var top = range.IsSettled
                ? GetDateLabelBounds(range.FirstDateIndex).Center.Y
                : range.Top;
            var bottom = range.IsSettled
                ? GetDateLabelBounds(range.LastDateIndex).Center.Y
                : range.Bottom;
            if (bottom < 0d || top > Size.Height) { continue; }

            var bracketRight = bracketLeft + range.Width;
            target.DrawPath(
            [
                new SMoveTo(bracketLeft, top),
                new SLineTo(bracketRight, top),
                new SLineTo(bracketRight, bottom),
                new SLineTo(bracketLeft, bottom)
            ], BracketStyle);
        }
    }

    private void DrawSecondaryLabels(IRenderTarget target, int firstVisibleIndex, int lastVisibleIndex)
    {
        for (var dateIndex = firstVisibleIndex; dateIndex <= lastVisibleIndex; dateIndex++)
        {
            var text = secondaryLabels[dateIndex];
            if (text is null) { continue; }

            var dateBounds = GetDateLabelBounds(dateIndex);
            var bracketWidth = BracketWidthForDate(dateIndex);
            var labelLeft = dateBounds.Right + bracketWidth + SecondaryLabelMargin;
            var labelBounds = new SRectF(
                labelLeft,
                dateBounds.Top,
                Size.Width - labelLeft,
                dateBounds.Height);

            target.DrawText(
                text,
                DateFont,
                labelBounds,
                SColor.White.WithOpacity(secondaryLabelOpacities[dateIndex]),
                SAngle.Zero,
                Alignment.LeftCenter);
        }
    }

    private double BracketWidthForDate(int dateIndex)
    {
        var range = bracketedRanges.FirstOrDefault(r =>
            dateIndex >= r.FirstDateIndex && dateIndex <= r.LastDateIndex);
        return range?.Width ?? DefaultBracketWidth;
    }

    private void StartRangeBeat(
        int beat,
        int firstDateIndex,
        int lastDateIndex,
        string firstDateLabel)
    {
        currentBeat = beat;
        rangeBeatElapsedSeconds = 0d;
        rangeBeatStartingListTop = listTop;
        rangeBeatFirstDateIndex = firstDateIndex;
        rangeBeatLastDateIndex = lastDateIndex;
        rangeBeatFirstDateLabel = firstDateLabel;

        if (activeBracket is not null)
        {
            activeBracket.IsSettled = true;
        }

        var arrowCenterY = Size.Height / 2d;
        activeBracket = new BracketedRange(firstDateIndex, lastDateIndex, DefaultBracketWidth)
        {
            Top = arrowCenterY,
            Bottom = arrowCenterY
        };
        bracketedRanges.Add(activeBracket);
    }

    private void UpdateBeatSeven(double deltaSeconds)
    {
        beatSevenElapsedSeconds += deltaSeconds;
        var progress = Math.Clamp(
            beatSevenElapsedSeconds / BeatSevenDurationSeconds,
            0d,
            1d);
        var easedProgress = Easings.Smoothstep(progress);

        contentHorizontalOffset = BeatSevenShiftX * easedProgress;
        arrow!.Position = beatSevenArrowStart.Move(contentHorizontalOffset, 0d);
        monthCalendar!.Opacity = easedProgress;
    }

    private void StartBeatTwelve()
    {
        currentBeat = 12;
        beatTwelveElapsedSeconds = 0d;
        beatTwelveContentStartX = contentHorizontalOffset;
        beatTwelveArrowStart = arrow!.Position;
        beatTwelveCalendarStartBounds = monthCalendar!.Bounds;
        weekdayCalendars[3] = monthCalendar;

        for (var weekdayIndex = 0; weekdayIndex < weekdayCalendars.Length; weekdayIndex++)
        {
            if (weekdayIndex == 3) { continue; }

            var targetBounds = CalendarTargetBounds(weekdayIndex);
            var calendar = new MonthCalendar(
                "November",
                WeekdayForIndex(weekdayIndex),
                30,
                DateFont,
                SColor.White)
            {
                Position = targetBounds.Position,
                Size = targetBounds.Size,
                Opacity = 0d
            };
            weekdayCalendars[weekdayIndex] = calendar;
            Add([calendar]);
        }
    }

    private void UpdateBeatTwelve(double deltaSeconds)
    {
        beatTwelveElapsedSeconds += deltaSeconds;
        var progress = Math.Clamp(
            beatTwelveElapsedSeconds / BeatTwelveDurationSeconds,
            0d,
            1d);
        var departureProgress = Easings.LiftOff(progress);
        var layoutProgress = Easings.Smoothstep(progress);

        contentHorizontalOffset = MathHelpers.Ease(
            beatTwelveContentStartX,
            BeatTwelveContentExitX,
            progress,
            Easings.LiftOff);
        var arrowExitDistance = BeatTwelveContentExitX - beatTwelveContentStartX;
        arrow!.Position = beatTwelveArrowStart.Move(
            arrowExitDistance * departureProgress,
            0d);

        var thursdayTarget = CalendarTargetBounds(3);
        var thursdayBounds = SRectF.Lerp(
            beatTwelveCalendarStartBounds,
            thursdayTarget,
            layoutProgress);
        monthCalendar!.Position = thursdayBounds.Position;
        monthCalendar.Size = thursdayBounds.Size;

        for (var weekdayIndex = 0; weekdayIndex < weekdayCalendars.Length; weekdayIndex++)
        {
            if (weekdayIndex == 3) { continue; }
            weekdayCalendars[weekdayIndex]!.Opacity = layoutProgress;
        }
    }

    private void StartBeatThirteen()
    {
        currentBeat = 13;
        beatThirteenElapsedSeconds = 0d;
        for (var weekdayIndex = 0; weekdayIndex < weekdayCalendars.Length; weekdayIndex++)
        {
            beatThirteenCalendarStartBounds[weekdayIndex] = weekdayCalendars[weekdayIndex]!.Bounds;
        }
    }

    private void UpdateBeatThirteen(double deltaSeconds)
    {
        beatThirteenElapsedSeconds += deltaSeconds;
        var progress = Math.Clamp(
            beatThirteenElapsedSeconds / BeatThirteenDurationSeconds,
            0d,
            1d);

        for (var weekdayIndex = 0; weekdayIndex < weekdayCalendars.Length; weekdayIndex++)
        {
            var calendar = weekdayCalendars[weekdayIndex]!;
            var thanksgivingDay = ThanksgivingDayFor(calendar.FirstDayOfWeek);
            var destinationIndex = thanksgivingDay - 22;
            var bounds = MathHelpers.Ease(
                beatThirteenCalendarStartBounds[weekdayIndex],
                CalendarTargetBounds(destinationIndex),
                progress,
                Easings.Smoothstep);
            calendar.Position = bounds.Position;
            calendar.Size = bounds.Size;
        }
    }

    private SRectF CalendarTargetBounds(int weekdayIndex)
    {
        var calendarSize = new SSizeF(CalendarGridWidth, CalendarGridHeight);
        if (weekdayIndex < 4)
        {
            var totalWidth = (4d * CalendarGridWidth) + (3d * CalendarGridHorizontalGap);
            var left = (Size.Width - totalWidth) / 2d;
            return calendarSize.At(new SPointF(
                left + (weekdayIndex * (CalendarGridWidth + CalendarGridHorizontalGap)),
                CalendarGridTopY));
        }

        var bottomIndex = weekdayIndex - 4;
        var bottomWidth = (3d * CalendarGridWidth) + (2d * CalendarGridHorizontalGap);
        var bottomLeft = (Size.Width - bottomWidth) / 2d;
        return calendarSize.At(new SPointF(
            bottomLeft + (bottomIndex * (CalendarGridWidth + CalendarGridHorizontalGap)),
            CalendarGridBottomY));
    }

    private static DayOfWeek WeekdayForIndex(int weekdayIndex) =>
        weekdayIndex == 6
            ? DayOfWeek.Sunday
            : (DayOfWeek)((int)DayOfWeek.Monday + weekdayIndex);

    private static int ThanksgivingDayFor(DayOfWeek firstDayOfWeek)
    {
        var mondayFirstIndex = ((int)firstDayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var firstThursday = 1 + ((3 - mondayFirstIndex + 7) % 7);
        return firstThursday + 21;
    }

    private void DrawBeatEightOrdinals(IRenderTarget target)
    {
        if (currentBeat < 8 || monthCalendar is null) { return; }

        DrawOrdinal(target, "4", dayNumber: 29, beatEightElapsedSeconds, delaySeconds: 0d, OrdinalColor, column: 0);
        DrawOrdinal(target, "3", dayNumber: 22, beatEightElapsedSeconds, delaySeconds: BeatEightDurationSeconds / 3d, OrdinalColor, column: 0);
        DrawOrdinal(target, "2", dayNumber: 15, beatEightElapsedSeconds, delaySeconds: BeatEightDurationSeconds * 2d / 3d, OrdinalColor, column: 0);
        DrawOrdinal(target, "1", dayNumber: 8, beatEightElapsedSeconds, delaySeconds: BeatEightDurationSeconds, OrdinalColor, column: 0);
    }

    private void DrawBeatNineOrdinals(IRenderTarget target)
    {
        if (currentBeat < 9 || monthCalendar is null) { return; }

        DrawOrdinal(target, "1", dayNumber: 1, beatNineElapsedSeconds, delaySeconds: 0d, ReverseOrdinalColor, column: 1);
        DrawOrdinal(target, "2", dayNumber: 8, beatNineElapsedSeconds, delaySeconds: BeatEightDurationSeconds / 3d, ReverseOrdinalColor, column: 1);
        DrawOrdinal(target, "3", dayNumber: 15, beatNineElapsedSeconds, delaySeconds: BeatEightDurationSeconds * 2d / 3d, ReverseOrdinalColor, column: 1);
        DrawOrdinal(target, "4", dayNumber: 22, beatNineElapsedSeconds, delaySeconds: BeatEightDurationSeconds, ReverseOrdinalColor, column: 1);
    }

    private void DrawOrdinal(
        IRenderTarget target,
        string text,
        int dayNumber,
        double elapsedSeconds,
        double delaySeconds,
        SColor color,
        int column)
    {
        var localProgress = (elapsedSeconds - delaySeconds)
            / BeatEightOrdinalEntranceDurationSeconds;
        if (localProgress < 0d) { return; }

        var transition = Transitions.SlideFadeIn(
            localProgress,
            new SPointF(BeatEightOrdinalEntranceDistance, 0d),
            Easings.Land);
        var dayBounds = monthCalendar!.GetDayTextBounds(dayNumber);
        const double ordinalWidth = 36d;
        const double ordinalMargin = 12d;
        var columnOffset = column * (ordinalWidth + OrdinalColumnGap);
        var bounds = new SRectF(
            monthCalendar.Bounds.Left - ordinalMargin - ordinalWidth - columnOffset,
            dayBounds.Top,
            ordinalWidth,
            dayBounds.Height) + transition.Offset;
        var beatTwelveOpacity = currentBeat >= 12
            ? 1d - Easings.Smoothstep(Math.Clamp(
                beatTwelveElapsedSeconds / BeatTwelveDurationSeconds,
                0d,
                1d))
            : 1d;
        target.DrawText(
            text,
            DateFont,
            bounds,
            color.WithOpacity(transition.Opacity * beatTwelveOpacity),
            SAngle.Zero,
            Alignment.Center);
    }

    private void UpdateRangeBeat(double deltaSeconds)
    {
        rangeBeatElapsedSeconds += deltaSeconds;
        var scrollProgress = Math.Clamp(
            rangeBeatElapsedSeconds / SlidingListArrowElement.ArrivalDurationSeconds,
            0d,
            1d);
        listTop = MathHelpers.Ease(
            rangeBeatStartingListTop,
            ListTopForCenteredDate(rangeBeatFirstDateIndex),
            scrollProgress,
            DateListEasing);

        var arrowCenterY = Size.Height / 2d;
        for (var dateIndex = rangeBeatFirstDateIndex;
             dateIndex <= rangeBeatLastDateIndex;
             dateIndex++)
        {
            if (GetDateLabelBounds(dateIndex).Center.Y >= arrowCenterY)
            {
                HighlightedDates[dateIndex] = true;
            }
        }

        for (var dateIndex = rangeBeatLastDateIndex - 1;
             dateIndex >= rangeBeatFirstDateIndex;
             dateIndex--)
        {
            if (GetDateLabelBounds(dateIndex).Center.Y < arrowCenterY) { continue; }

            var text = dateIndex == rangeBeatFirstDateIndex
                ? rangeBeatFirstDateLabel!
                : WeekdayAbbreviation(dateIndex);
            StartSecondaryLabelFade(dateIndex, text);
        }

        if (activeBracket is null) { return; }

        activeBracket.Top = arrowCenterY;
        activeBracket.Bottom = GetDateLabelBounds(rangeBeatLastDateIndex).Center.Y;
        activeBracket.IsSettled = scrollProgress >= 1d;
    }

    private void StartSecondaryLabelFade(int dateIndex, string text)
    {
        if (secondaryLabels[dateIndex] is not null) { return; }

        secondaryLabels[dateIndex] = text;
        secondaryLabelFadeElapsedSeconds[dateIndex] = 0d;
        secondaryLabelOpacities[dateIndex] = 0d;
        secondaryLabelsFading[dateIndex] = true;
    }

    private void UpdateSecondaryLabelFades(double deltaSeconds)
    {
        for (var dateIndex = 0; dateIndex < DateCount; dateIndex++)
        {
            if (!secondaryLabelsFading[dateIndex]) { continue; }

            secondaryLabelFadeElapsedSeconds[dateIndex] += deltaSeconds;
            var progress = Math.Clamp(
                secondaryLabelFadeElapsedSeconds[dateIndex] / SecondaryLabelFadeDurationSeconds,
                0d,
                1d);
            secondaryLabelOpacities[dateIndex] = Easings.Smoothstep(progress);
            if (progress >= 1d)
            {
                secondaryLabelsFading[dateIndex] = false;
            }
        }
    }

    private static string WeekdayAbbreviation(int dateIndex)
    {
        // November 29 was the fourth Thursday in 2018, matching this slide's premise.
        var date = new DateOnly(2018, 1, 1).AddDays(dateIndex);
        return date.ToString("ddd", CultureInfo.InvariantCulture);
    }

    private double InitialListTop => -(DateCount * DateRowHeight);

    private double ListTopForCenteredDate(int dateIndex) =>
        (Size.Height / 2d)
        - (dateIndex * DateRowHeight)
        - DateTopMargin
        - (DateLabelHeight / 2d);

    private static string[] BuildDateLabels()
    {
        var labels = new string[DateCount];
        var date = new DateOnly(2025, 1, 1);
        for (var dateIndex = 0; dateIndex < labels.Length; dateIndex++)
        {
            labels[dateIndex] = date.ToString("MMMM d", CultureInfo.InvariantCulture);
            date = date.AddDays(1);
        }

        return labels;
    }
}
