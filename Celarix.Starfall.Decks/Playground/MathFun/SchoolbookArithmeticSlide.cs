using Celarix.Starfall.Atria;
using Celarix.Starfall.Atria.Elements;
using Celarix.Starfall.Audio;
using Celarix.Starfall.Mathematics.Schoolbook;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Decks.Playground.MathFun
{
    /// <summary>
    /// Works through a few problems by hand: each Advance does one column, and plays a tone pitched
    /// by the digit it writes. After a problem finishes, the next Advance moves to the next problem.
    /// </summary>
    public sealed class SchoolbookArithmeticSlide : AtriaSlide
    {
        private const double ToneSeconds = 0.15d;
        private static readonly SColor _backgroundColor = new(8, 0, 130, 255);
        private static readonly PitchMapper _pitch = new();

        private readonly SchoolbookWork[] _problems =
        [
            SchoolbookWork.Addition(4738, 2594),
            SchoolbookWork.Subtraction(5003, 1867),
            SchoolbookWork.Multiplication(487, 36),
            SchoolbookWork.Addition(0b1011, 0b0111, numberBase: 2),
        ];

        private int _problemIndex;
        private SchoolbookArithmeticElement? _work;
        private BasisPoint? _workAnchor;
        private TextBlock? _title;
        private TextBlock? _caption;

        public SchoolbookArithmeticSlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size)
        {
        }

        public override string CurrentBeat => $"{TitleFor(_work?.Work)}: {CaptionFor(_work)}";

        public override string Notes =>
            "Advance works one column at a time; Rewind steps back. Four problems: addition with carries, "
            + "subtraction borrowing across zeros, long multiplication, and binary addition.";

        public override void Initialize()
        {
            BackgroundColor = _backgroundColor;

            _workAnchor = new BasisPoint(Center, "#schoolbookAnchor");
            var titleAnchor = new BasisPoint(new SPointF(Center.X, Size.Height * 0.1d), "#schoolbookTitleAnchor");
            var captionAnchor = new BasisPoint(new SPointF(Center.X, Size.Height * 0.9d), "#schoolbookCaptionAnchor");

            _title = new TextBlock("#schoolbookTitle") { FontFamily = "Calibri", FontSize = 64d, Color = SColor.White, Opacity = 1d };
            _title.AnchorCenterTo(titleAnchor);
            _caption = new TextBlock("#schoolbookCaption") { FontFamily = "Calibri", FontSize = 40d, Color = SColor.White, Opacity = 1d };
            _caption.AnchorCenterTo(captionAnchor);
            Add([_workAnchor, titleAnchor, captionAnchor, _title, _caption]);

            ShowProblem(0, atEnd: false);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_work!.StepForward())
            {
                PlayCurrentTone();
                UpdateCaption();
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
            if (_work != null) { Remove([_work]); }

            _problemIndex = index;
            var work = _problems[index];
            _work = new SchoolbookArithmeticElement($"#schoolbook{index}", work)
            {
                Size = new SSizeF(Size.Width * 0.6d, Size.Height * 0.6d),
                StepIndex = atEnd ? work.Steps.Count - 1 : -1
            };
            _work.AnchorCenterTo(_workAnchor!);
            Add([_work]);

            _title!.SetTextAndKeepFontSize(MeasurementService, TitleFor(work));
            UpdateCaption();
        }

        private void UpdateCaption() => _caption!.SetTextAndKeepFontSize(MeasurementService, CaptionFor(_work));

        private void PlayCurrentTone()
        {
            if (_work?.CurrentStep?.Tone is { } tone)
            {
                Runtime.Tones.PlayTone(_pitch.FrequencyFor(tone, _work.Work.ToneCount), ToneSeconds);
            }
        }

        private static string TitleFor(SchoolbookWork? work) => work switch
        {
            null => string.Empty,
            { NumberBase: 10 } => work.Title,
            _ => $"{work.Title} (base {work.NumberBase})"
        };

        private static string CaptionFor(SchoolbookArithmeticElement? element) => element switch
        {
            null => string.Empty,
            { CurrentStep: { } step } => step.Description,
            _ => "Press Advance to work it column by column."
        };
    }
}
