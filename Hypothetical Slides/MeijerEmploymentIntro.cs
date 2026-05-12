namespace HypotheticalSlides;

public sealed class MeijerEmploymentIntro : AtriaSlide
{
    private class DayTypeDisplay : AtriaElement
    {
        private readonly SColor[] _typeColors;
        private readonly int[] _dayTypes;
        public float RevealedPart { get; set; } = 0;

        public DayTypeDisplay(int[] dayTypes)
        {
            _dayTypes = dayTypes;
            _typeColors = new[]
            {
                Colors.Orange,    // Working
                Colors.Blue,      // Non-working
                Colors.LightBlue, // Paid time off
                Colors.LightGreen // Unpaid time off
            };
        }

        public override List<HeliumRenderable> GetRenderables()
        {
            var revealedPixels = (int)(RevealedPart * _dayTypes.Length);
            var renderables = new List<HeliumRenderable>();

            for (int i = 0; i < revealedPixels; i++)
            {
                var color = _typeColors[_dayTypes[i]];
                renderables.Add(new RectangleRenderable
                {
                    Bounds = new SRect(Position.Left + i,
                        Position.Top,
                        1, // each day is 1 pixel wide
                        Size.Height),
                    Fill = color
                });
            }

            return renderables;
        }
    }

    internal enum State
    {
        Initial,
        ShowTitle,
        ShowCenterDot,
        ExpandTimeline,
        AddYearTicks,
        AddMonthTicks,
        AddDaysAndLegend,
        FadeOut
    }

    private const int CircleWidth = 20;
    private static readonly DateOnly StartDate = new(2013, 9, 17);
    private static readonly DateOnly EndDate = new(2018, 5, 19);
    private static readonly int TotalDays = EndDate.DayNumber - StartDate.DayNumber;

    private readonly StateMachine<State> _stateMachine;

    public override void Initialize()
    {
        DebugMode.SetStateImmediate(State.AddYearTicks);
        DebugMode.ShowAnchors = true;

        this.BackgroundColor = Colors.DarkBlue;
		_stateMachine = new(this, State.Initial);
    }

	[StateTransition(State.Initial, State.ShowTitle)]
    private void ToShowTitle()
    {
        var topText = new TextBlock("#topText")
        {
            Text = "Meijer Employment Statistics",
            FontSize = 48,
            Foreground = Color.White,
            FontFamily = "Calibri"
        };
        var bottomText = new TextBlock("#bottomText")
        {
            Text = "A Look at the Data",
            FontSize = 32,
            Foreground = Color.White,
            FontFamily = "Calibri"
        };

        // Figure out placement
        var vCenterBasis = new BasisLine(TopCenter, BottomCenter);
        var topTextHeight = topText.MeasureText().Height;
        var margin = topTextHeight / 2; // mess with this until you like the spacing

        var topTextAnchor = = new BasisPoint(vCenterBasis.Center
                .Up(margin)
                .Up(topTextHeight / 2), "#topTextAnchor");   // with ID takes a CSS selector as input - #word means id="word" and you can .have .multiple .classes .too
        topText.AnchorCenterTo(topTextAnchor);
        var bottomTextAnchor = BasisPoint.From(vCenterBasis.Center
                .Down(margin)
                .Down(topTextHeight / 2))
            .WithId("#bottomTextAnchor");
        bottomText.AnchorCenterTo(bottomTextAnchor);

        Add([topText, bottomText, topTextAnchor, bottomTextAnchor])
            .AnimateBasic(1.0, AnimationTypes.FadeIn, Easings.Linear);
    }

    private void ToShowCenterDot()
    {
        var centerDot = new Ellipse("#leftDot") // not sic! this is the first dot of two
        {
            Width = CircleWidth,     // you know what, sure, absolute positioning is fine here
            Height = CircleWidth,    // maybe Atria can let things start with absolute stuff
                                     // and then everything that follows is relative
            Fill = Color.White
        };
        centerDot.CenterOn(Center);
        Add(centerDot)
            .AnimateBasic(1.0, AnimationType.Grow, Easing.Linear);

        var topTextAnchor = Get<BasisPoint>("#topTextAnchor");
        var bottomTextAnchor = Get<BasisPoint>("#bottomTextAnchor");
        var distanceBetweenAnchors = topTextAnchor.Position.Y - bottomTextAnchor.Position.Y;

        // We want to move the text such that each block has that distance
        // from the top/bottom of the slide.
        var vCenterBasis = new BasisLine(TopCenter, BottomCenter);
        topTextAnchor.AnimateTo(a => a.Position, Easing.Linear, 1.0d, vCenterBasis.Top.Down(distanceBetweenAnchors));
        bottomTextAnchor.AnimateTo(a => a.Position, Easing.Linear, 1.0d, vCenterBasis.Bottom.Up(distanceBetweenAnchors));
    }

    private void ToExpandTimeline()
    {
        var leftDot = Get<Ellipse>("#leftDot");
        var rightDot = new Ellipse("#rightDot")
        {
            Width = CircleWidth,
            Height = CircleWidth,
            Fill = Color.White
        };
        rightDot.CenterOn(Center);
        Add(rightDot);  // add immediately with no fade-in

        var leftBoundingBox = leftDot.Bounds;
        var rightBoundingBox = rightDot.Bounds;
        var desiredDistanceBetweenBoxes = TotalDays;
        var leftNewRight = Center.X - desiredDistanceBetweenBoxes / 2;
        var rightNewLeft = Center.X + desiredDistanceBetweenBoxes / 2;
        var leftTarget = leftDot.Position.WithX(leftNewRight - leftBoundingBox.Width);
        var rightTarget = rightDot.Position.WithX(rightNewLeft);

        leftDot.Animate(a => a.Position, 1.0d, Easing.Linear)
            .To(leftTarget);
        rightDot.Animate(a => a.Position, 1.0d, Easing.Linear)
            .To(rightTarget);

        // Draw line connecting the dots
        var timeline = new Line("#timeline")
        {
            Stroke = Color.White,
            StrokeThickness = 4,
            X1 = Center.X,
            Y1 = Center.Y,
            X2 = Center.X,
            Y2 = Center.Y
        };
        Add(timeline);
        timeline.Animate(a => (a.X1, a.Y1), 1.0d, Easing.Linear)
            .To((leftTarget.X + CircleWidth / 2, Center.Y));
        timeline.Animate(a => (a.X2, a.Y2), 1.0d, Easing.Linear)
            .To((rightTarget.X, Center.Y));
    }

    private void ToAddYearTicks()
    {
        var timeline = Get<Line>("#timeline");

        var tickHeight = CircleHeight / 2;
        var tickY = Center.Y;
        var tickWidth = 4;

        for (var date = StartDate; date <= EndDate; date = date.AddYears(1))
        {
            var daysFromStart = date.DayNumber - StartDate.DayNumber;
            var tickX = timeline.X1 + (timeline.X2 - timeline.X1) * daysFromStart / TotalDays;

            var tick = new Line(".yearTick")
            {
                Stroke = Color.White,
                StrokeThickness = tickWidth,
                X1 = tickX,
                Y1 = tickY - tickHeight,
                X2 = tickX,
                Y2 = tickY + tickHeight
            };
            Add(tick);
        }

        Animate<Line>(".yearTick", t => t.Y1, AnimationType.FadeIn, Easing.Linear)
            .To(t => t.Y1 - tickHeight);    // animate the year ticks to grow upwards from the timeline

        // Add year labels below the timeline centered on the block of the timeline that corresponds
        // to that year.
        var yearLabelY = Center.Down(CircleHeight / 2).Y;
        var yearRanges = new List<(int, int)>();
        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            // okay I think you can figure out the logic here
        }
        var left = new Point(timeline.X1, yearLabelY);
        var centers = yearRanges.Select(r => Helpers.Midpoint(r.Item1, r.Item2))
            .Select(c => BasisPoint.From(left.Right(c)).WithId(".yearLabelAnchor"));
        var yearLabels = yearRanges.Select((r, i) => (r.Item1, i + 2013))
            .Select(t => new TextBlock($"#yearLabel{t.Item2} .yearLabel")
            {
                Text = t.Item2.ToString(),
                FontSize = 16,
                Foreground = Color.White,
                FontFamily = "Calibri"
            });
        foreach (var (center, label) in centers.Zip(yearLabels))
        {
            label.AnchorCenterTo(center);
            Add(label, center);
        }
        Add(centers);
        Add(yearLabels);
        Animate(".yearLabel", 1.0d, AnimationType.FadeIn, Easing.Linear);
    }

    private void ToAddMonthTicks()
    {
        var timeline = Get<Line>("#timeline");

        var tickHeight = CircleHeight / 4;
        var tickY = Center.Y;
        var tickWidth = 2;

        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            if (date.Day != 1) continue; // only add ticks for the first day of each month

            var daysFromStart = date.DayNumber - StartDate.DayNumber;
            var tickX = timeline.X1 + (timeline.X2 - timeline.X1) * daysFromStart / TotalDays;

            var tick = new Line(".monthTick")
            {
                Stroke = Color.White,
                StrokeThickness = tickWidth,
                X1 = tickX,
                Y1 = tickY - tickHeight,
                X2 = tickX,
                Y2 = tickY + tickHeight
            };
            Add(tick);
        }

        Animate<Line>(".monthTick", t => t.Y1, AnimationType.FadeIn, Easing.Linear)
            .To(t => t.Y1 - tickHeight);    // animate the month ticks to grow upwards from the timeline

        // Add month labels above the timeline centered on the block of the timeline that corresponds
        // to that month.
        var monthLabelY = Center.Up(CircleHeight / 3).Y;
        var monthRanges = new List<(int, int)>();
        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            // okay I think you can figure out the logic here
        }
        var left = new Point(timeline.X1, monthLabelY);
        var centers = monthRanges.Select(r => Helpers.Midpoint(r.Item1, r.Item2))
            .Select(c => BasisPoint.From(left.Right(c)).WithId(".monthLabelAnchor"));
        var monthLabels = monthRanges.Select((r, i) => (r.Item1, i + 1))
            .Select(t => new TextBlock($"#monthLabel{t.Item2} .monthLabel")
            {
                Text = t.Item2.ToString(),  // okay yes this should be logic that prints "J", "F", etc.
                FontSize = 16,
                Foreground = Color.LightGray.WithAlpha(0.3f),
                FontFamily = "Calibri"
            });
        foreach (var (center, label) in centers.Zip(monthLabels))
        {
            label.AnchorCenterTo(center);
            Add(label, center);
        }
        Add(centers);
        Add(monthLabels);
        Animate(".monthLabel", 1.0d, AnimationType.FadeIn, Easing.Linear);
    }

    private HeliumRenderable? _legend;
    private void ToAddDaysAndLegend()
    {
        var timeline = Get<Line>("#timeline");
        var dayTypes = MagicHypotheticalClass.GetDayTypesForMeijerEmployment(); // this is a list of integers where each integer corresponds to a type of day (working, non-working, paid time off, unpaid time off)
        var dayTypeDisplay = new DayTypeDisplay(dayTypes)
        {
            Position = new Point(timeline.X1, timeline.Y1 - CircleHeight / 2),
            Size = new Size(timeline.X2 - timeline.X1, CircleHeight)
        };
        Add(dayTypeDisplay);
        Animate(dayTypeDisplay, d => d.RevealedPart, 1.0d, AnimationType.Smoothstep)
            .To(1.0d);    // animate the days being revealed from left to right

        // Add legend
        var legendItems = new[]
        {
            (Color.Orange, "Working"),
            (Color.Blue, "Non-working"),
            (Color.LightBlue, "Paid time off"),
            (Color.LightGreen, "Unpaid time off")
        };

        var legendSpace = new AtriaSpace(backgroundColor: Color.Transparent);
        var colorSquareSize = 20;
        var legendItemSpacing = 10;
        var gridSpacer = legendSpace.AddGridSpacer(".legendItem", legendItemSpacing, rows: null, columns: 2);
        
        foreach (var item in legendItems)
        {
            gridSpacer.Add(new Rectangle
            {
                Fill = item.Item1,
                BorderThickness = 1,
                BorderColor = Color.White,
            });
            gridSpacer.Add(new TextBlock
            {
                Text = item.Item2,
                FontSize = 16,
                Foreground = Color.White,
                FontFamily = "Calibri"
            });
        }

        _legend = legendSpace.Render("#legend");
        var legendMargin = _legend.Size.Width / 4;
        _legend.BottomRightAt(BottomRight.Left(legendMargin).Up(legendMargin));
        Add(_legend);
    }

    private void ToFadeOut()
    {
        Animate("*", 1.0d, AnimationType.FadeOut, Easing.Linear);
    }
}