using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Playground.AtriaTests;

internal sealed class ThanksgivingContradictionSlide : AtriaSlide
{
    private sealed class Diagram : AtriaElement
    {
        private static readonly SColor SoftWhite = new(255, 255, 255, 150);
        private static readonly SColor Contradiction = new(255, 125, 105, 255);
        private static readonly SColor Thanksgiving = new(255, 190, 75, 255);
        private static readonly SFont TitleFont = BoldFont(38f);
        private static readonly SFont MonthFont = BoldFont(18f);
        private static readonly SFont DayFont = BoldFont(72f);
        private static readonly SFont LabelFont = new SFontFamily("Calibri", 23f);
        private static readonly SFont SmallFont = new SFontFamily("Calibri", 18f);
        private static readonly SFont ConclusionFont = BoldFont(42f);

        public double Step { get; set; }
        public Diagram() { Id = AtriaId.Parse("#contradictionDiagram"); }

        public override void Render(IRenderTarget target)
        {
            DrawText(target, "A proof by contradiction", TitleFont, new SRectF(56, 36, Size.Width - 112, 50), SColor.White);
            var finalOpacity = Reveal(5.65, 6d);
            var chainOpacity = 1d - (finalOpacity * 0.72d);
            DrawChain(target, chainOpacity);
            DrawContradiction(target, chainOpacity);
            DrawFifthThursdayConclusion(target, chainOpacity);
            DrawThanksgivingConclusion(target, finalOpacity);
        }

        private void DrawChain(IRenderTarget target, double chainOpacity)
        {
            var xs = new[] { 128d, 358d, 588d, 818d, 1048d };
            var days = new[] { "29", "22", "15", "8", "1" };
            var ordinals = new[] { "4th Thursday?", "3rd Thursday", "2nd Thursday", "1st Thursday" };
            var starts = new[] { -1d, 0d, 1d, 2d, 3d };

            for (var i = 0; i < days.Length; i++)
            {
                var alpha = Reveal(starts[i], starts[i] + 0.7d) * chainOpacity;
                if (alpha <= 0d) { continue; }
                if (i > 0) { DrawMinusSeven(target, xs[i - 1] + 78, xs[i] - 78, alpha); }

                var card = new SRectF(xs[i] - 70, 197, 140, 190);
                var stroke = i == 4 && Step >= 4d && Step < 6d ? Contradiction
                    : i == 1 && Step >= 6d ? Thanksgiving : SoftWhite;
                target.DrawRectangle(card, stroke.WithOpacity(alpha), SPaintStyle.Stroke, SAngle.Zero);
                DrawText(target, "NOV", MonthFont, new SRectF(card.Left, card.Top + 18, card.Width, 26), SColor.White.WithOpacity(alpha));
                DrawText(target, days[i], DayFont, new SRectF(card.Left, card.Top + 49, card.Width, 82), SColor.White.WithOpacity(alpha));

                if (i < 4)
                {
                    var label = i == 0 && Step >= 5d ? "5th Thursday" : ordinals[i];
                    DrawText(target, label, LabelFont, new SRectF(xs[i] - 105, card.Bottom + 16, 210, 34), SColor.White.WithOpacity(alpha));
                }
            }

            var expected = Reveal(2.65, 3d) * (1d - Reveal(3.7, 4d)) * chainOpacity;
            if (expected <= 0d) { return; }
            for (var y = 153d; y < 477d; y += 22d)
            {
                target.DrawLine(new SPointF(933, y), new SPointF(933, y + 11), SColor.White.WithOpacity(expected * 0.75d), 3f);
            }
            DrawText(target, "OCTOBER", LabelFont, new SRectF(948, 130, 230, 38), SColor.White.WithOpacity(expected));
            DrawText(target, "the previous month", SmallFont, new SRectF(948, 166, 230, 30), SoftWhite.WithOpacity(expected));
        }

        private void DrawContradiction(IRenderTarget target, double chainOpacity)
        {
            var alpha = Reveal(3.6, 4d) * (1d - Reveal(4.7, 5d)) * chainOpacity;
            if (alpha <= 0d) { return; }
            target.DrawLine(new SPointF(954, 151), new SPointF(1190, 151), Contradiction.WithOpacity(alpha), 5f);
            DrawText(target, "STILL NOVEMBER", LabelFont, new SRectF(930, 112, 290, 34), SColor.White.WithOpacity(alpha));
            DrawText(target, "contradiction", ConclusionFont, new SRectF(390, 535, 500, 62), SColor.White.WithOpacity(alpha));
        }

        private void DrawFifthThursdayConclusion(IRenderTarget target, double chainOpacity)
        {
            var alpha = Reveal(4.65, 5d) * (1d - Reveal(5.7, 6d)) * chainOpacity;
            if (alpha <= 0d) { return; }
            DrawText(target, "Five Thursdays", ConclusionFont, new SRectF(390, 525, 500, 58), SColor.White.WithOpacity(alpha));
            DrawText(target, "1  ·  8  ·  15  ·  22  ·  29", LabelFont, new SRectF(365, 584, 550, 42), SoftWhite.WithOpacity(alpha));
        }

        private void DrawThanksgivingConclusion(IRenderTarget target, double alpha)
        {
            if (alpha <= 0d) { return; }
            DrawText(target, "THANKSGIVING", LabelFont, new SRectF(258, 142, 200, 36), SColor.White.WithOpacity(alpha));
            target.DrawLine(new SPointF(287, 180), new SPointF(348, 203), Thanksgiving.WithOpacity(alpha), 4f);
            DrawText(target, "Thanksgiving cannot be November 29", ConclusionFont, new SRectF(210, 532, 860, 62), SColor.White.WithOpacity(alpha));
        }

        private static void DrawMinusSeven(IRenderTarget target, double fromX, double toX, double alpha)
        {
            var from = new SPointF(fromX, 301);
            var to = new SPointF(toX, 301);
            var color = SColor.White.WithOpacity(alpha * 0.72d);
            target.DrawLine(from, to, color, 3f);
            target.DrawLine(to, to.Move(-13, -8), color, 3f);
            target.DrawLine(to, to.Move(-13, 8), color, 3f);
            DrawText(target, "−7 days", SmallFont, new SRectF(fromX, 258, toX - fromX, 28), color);
        }

        private double Reveal(double from, double to)
        {
            if (Step <= from) { return 0d; }
            if (Step >= to) { return 1d; }
            return Easings.Smoothstep((Step - from) / (to - from));
        }

        private static SFont BoldFont(float size) => new SFontFamily("Calibri", size, FontWeight.Bold, FontWidth.Normal, FontSlant.Upright);
        private static void DrawText(IRenderTarget target, string text, SFont font, SRectF bounds, SColor color) =>
            target.DrawText(text, font, bounds, color, SAngle.Zero);
    }

    private const int LastStep = 6;
    private Diagram? diagram;
    private int currentStep;

    public ThanksgivingContradictionSlide(int width, int height) : base(width, height) { }

    public override void Initialize()
    {
        BackgroundColor = SColor.StarfallDefault;
        diagram = new Diagram { Position = SPointF.Zero, Size = Size, Step = 0d };
        Add([diagram]);
    }

    public override SlideAdvanceResult Advance()
    {
        if (currentStep >= LastStep) { return SlideAdvanceResult.CanAdvance; }
        currentStep += 1;
        diagram!.AnimateTo(element => ((Diagram)element).Step, Easings.Smoothstep, 0.7d, currentStep);
        return SlideAdvanceResult.InternalStateChanged;
    }

    public override SlideAdvanceResult Rewind()
    {
        if (currentStep <= 0) { return SlideAdvanceResult.CanRewind; }
        currentStep -= 1;
        diagram!.AnimateTo(element => ((Diagram)element).Step, Easings.Smoothstep, 0.45d, currentStep);
        return SlideAdvanceResult.InternalStateChanged;
    }
}
