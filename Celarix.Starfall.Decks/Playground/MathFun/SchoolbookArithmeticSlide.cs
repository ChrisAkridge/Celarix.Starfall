using Celarix.Starfall.Atria;
using Celarix.Starfall.Atria.Elements;
using Celarix.Starfall.Audio;
using Celarix.Starfall.Mathematics.Schoolbook;
using Celarix.Starfall.Rendering.Models;
using System.Numerics;

namespace Celarix.Starfall.Decks.Playground.MathFun
{
    /// <summary>
    /// Works through a few problems by hand: each Advance does one step, the table beside it lights
    /// up the fact being looked up, and each lookup plays a two-note chord (column and row). After a
    /// problem finishes, the next Advance moves to the next problem.
    /// </summary>
    public sealed class SchoolbookArithmeticSlide : AtriaSlide
    {
        private static readonly SColor _backgroundColor = new(8, 0, 130, 255);
        private static readonly SchoolbookSound _sound = new();

        /// <summary>How long auto-play takes to work a whole problem, however many steps it has.</summary>
        private const double AutoPlayTotalSeconds = 1d;

        /// <param name="Label">Shown after the problem in the title, e.g. "folded".</param>
        /// <param name="AutoPlay">Whether Advance plays the whole problem instead of stepping it.</param>
        /// <param name="ShowTable">Whether the addition/times table appears beside the work.</param>
        private sealed record Problem(SchoolbookWork Work, string? Label = null, bool AutoPlay = false, bool ShowTable = true);

        private readonly Problem[] _problems =
        [
            new(SchoolbookWork.Addition(4738, 2594)),
            new(SchoolbookWork.Subtraction(5003, 1867)),
            new(SchoolbookWork.Multiplication(487, 36), "digit pairs"),
            new(SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.Folded), "folded"),
            new(SchoolbookWork.Addition(0b1011, 0b0111, numberBase: 2), "base 2"),
            new(SchoolbookWork.Multiplication(BigInteger.Parse("31415926535897932"), BigInteger.Parse("27182818284590452"),
                layout: MultiplicationLayout.Folded), "folded", AutoPlay: true),
        ];

        private int _problemIndex;
        private SchoolbookArithmeticElement? _work;
        private OperationTableElement? _table;
        private BasisPoint? _workAnchor;
        private BasisPoint? _tableAnchor;
        private TextBlock? _title;
        private TextBlock? _caption;

        public SchoolbookArithmeticSlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size)
        {
        }

        public override string CurrentBeat => $"{TitleFor(_problems[_problemIndex])}: {CaptionFor(_work)}";

        public override string Notes =>
            "Advance works one step at a time; Rewind steps back. The table on the right shows each fact being "
            + "looked up, and each lookup plays a chord: the column at middle C + X, the row an octave up at C5 + Y. "
            + "Problems: addition with carries, subtraction borrowing across zeros, multiplication by digit pairs and "
            + "folded, binary addition, and a long folded multiplication that plays itself in about a second "
            + "(Advance again to finish it at once).";

        public override void Initialize()
        {
            BackgroundColor = _backgroundColor;

            _workAnchor = new BasisPoint(new SPointF(Size.Width * 0.36d, Center.Y), "#schoolbookAnchor");
            _tableAnchor = new BasisPoint(new SPointF(Size.Width * 0.81d, Center.Y), "#schoolbookTableAnchor");
            var titleAnchor = new BasisPoint(new SPointF(Center.X, Size.Height * 0.1d), "#schoolbookTitleAnchor");
            var captionAnchor = new BasisPoint(new SPointF(Center.X, Size.Height * 0.9d), "#schoolbookCaptionAnchor");

            _title = new TextBlock("#schoolbookTitle") { FontFamily = "Calibri", FontSize = 64d, Color = SColor.White, Opacity = 1d };
            _title.AnchorCenterTo(titleAnchor);
            _caption = new TextBlock("#schoolbookCaption") { FontFamily = "Calibri", FontSize = 40d, Color = SColor.White, Opacity = 1d };
            _caption.AnchorCenterTo(captionAnchor);
            Add([_workAnchor, _tableAnchor, titleAnchor, captionAnchor, _title, _caption]);

            ShowProblem(0, atEnd: false);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_work!.IsPlaying)
            {
                _work.ShowComplete();
                UpdateCaption();
                UpdateTable();
                return SlideAdvanceResult.InternalStateChanged;
            }
            if (!_work.IsComplete && _problems[_problemIndex].AutoPlay)
            {
                _work.Play(AutoPlayTotalSeconds / _work.Work.Steps.Count);
                return SlideAdvanceResult.InternalStateChanged;
            }
            if (_work.StepForward())
            {
                return SlideAdvanceResult.InternalStateChanged;
            }
            if (_problemIndex < _problems.Length - 1)
            {
                ShowProblem(_problemIndex + 1, atEnd: false);
                return SlideAdvanceResult.InternalStateChanged;
            }
            return SlideAdvanceResult.CanAdvance;
        }

        public override SlideAdvanceResult Rewind()
        {
            if (_work!.StepBack())
            {
                UpdateCaption();
                UpdateTable();
                return SlideAdvanceResult.InternalStateChanged;
            }
            if (_problemIndex > 0)
            {
                ShowProblem(_problemIndex - 1, atEnd: true);
                return SlideAdvanceResult.InternalStateChanged;
            }
            return SlideAdvanceResult.CanRewind;
        }

        private void ShowProblem(int index, bool atEnd)
        {
            if (_work != null)
            {
                _work.StepTaken -= Work_StepTaken;
                Remove([_work]);
            }
            if (_table != null)
            {
                Remove([_table]);
                _table = null;
            }

            _problemIndex = index;
            var problem = _problems[index];
            _work = new SchoolbookArithmeticElement($"#schoolbook{index}", problem.Work)
            {
                Size = new SSizeF(Size.Width * 0.6d, Size.Height * 0.6d),
                StepIndex = atEnd ? problem.Work.Steps.Count - 1 : -1
            };
            _work.StepTaken += Work_StepTaken;
            _work.AnchorCenterTo(_workAnchor!);
            Add([_work]);

            if (problem.ShowTable)
            {
                var tableSide = Math.Min(Size.Width * 0.3d, Size.Height * 0.6d);
                _table = new OperationTableElement($"#schoolbookTable{index}", numberBase: problem.Work.NumberBase)
                {
                    Size = new SSizeF(tableSide, tableSide)
                };
                _table.AnchorCenterTo(_tableAnchor!);
                Add([_table]);
            }
            UpdateTable();

            _title!.SetTextAndKeepFontSize(MeasurementService, TitleFor(problem));
            UpdateCaption();
        }

        private void UpdateCaption() => _caption!.SetTextAndKeepFontSize(MeasurementService, CaptionFor(_work));

        private void UpdateTable()
        {
            if (_table == null) { return; }
            var lookups = _work?.CurrentStep?.Lookups;
            _table.Lookup = lookups is { Count: > 0 } ? lookups[^1] : null;
        }

        private void Work_StepTaken(object? sender, SchoolbookStep step)
        {
            _sound.PlayStep(Runtime.Tones, step, _work!.SecondsPerStep);
            // Auto-play steps happen during Update, so keep the caption and table in step with them.
            UpdateCaption();
            UpdateTable();
        }

        private static string TitleFor(Problem problem) =>
            problem.Label is { } label ? $"{problem.Work.Title} ({label})" : problem.Work.Title;

        private static string CaptionFor(SchoolbookArithmeticElement? element) => element switch
        {
            null => string.Empty,
            { CurrentStep: { } step } => step.Description,
            _ => "Press Advance to work it column by column."
        };
    }
}
