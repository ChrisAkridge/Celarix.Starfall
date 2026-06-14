using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_04_NoEscapeFromInfiniteExpansions : AtriaSlide
    {
        internal enum State
        {
            Initial,
            ShowDividendAndDivisor,
            Step01,
            Step02,
            Step03,
            Step04,
            Step05,
            FinishProblem
        }

        private const string FontFamily = "Consolas";
        private const float FontSize = 48f;
        private const double LineMargin = 2d;
        private const double StrokeWidth = 4d;
        private const double InnerPadding = 16d;
        private StateMachine<State> _stateMachine;
        private double? _charWidth;
        private double? _lineHeight;

        public SlideFP_04_NoEscapeFromInfiniteExpansions(int width, int height) : base(width, height)
        {
        }

        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine.CurrentState == State.FinishProblem)
            {
                return SlideAdvanceResult.CanAdvance;
            }

            var nextState = _stateMachine.CurrentState + 1;
            _stateMachine.GoToState(nextState);
            return SlideAdvanceResult.InternalStateChanged;
        }

        public override SlideAdvanceResult Rewind()
        {
            if (_stateMachine.CurrentState == State.Initial)
            {
                return SlideAdvanceResult.CanRewind;
            }

            var previousState = _stateMachine.CurrentState - 1;
            _stateMachine.GoToState(previousState);
            return SlideAdvanceResult.InternalStateChanged;
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _stateMachine = new StateMachine<State>(this, State.Initial);

            SFontFamily font = new(FontFamily, FontSize);
            SSizeF measurement = MeasurementService.MeasureText("0", font);
            _lineHeight = measurement.Height + LineMargin;
            _charWidth = measurement.Width;
        }

        [StateTransition<State>(State.Initial, State.ShowDividendAndDivisor)]
        private void ToShowDividendAndDivisor()
        {
            var initialBasis = TopLeft.Right(InnerPadding).Down(InnerPadding);

            var divisorAnchor = new BasisPoint(initialBasis.Down(_lineHeight!.Value), "#divisorAnchor");
            var divisor = MakeTextBlock("#divisor", "3");
            divisor.AnchorTopLeftTo(divisorAnchor);

            var sideLineAnchor = new BasisPoint(divisorAnchor.Point.Right(_charWidth!.Value + (LineMargin * 4d)), "#sideLineAnchor");
            var sideLine = new LineElement("#sideLine")
            {
                StrokeColor = SColor.White,
                StrokeWidth = StrokeWidth,
                ToPoint = SPointF.Zero.Down(_lineHeight.Value)
            };
            sideLine.AnchorTopLeftTo(sideLineAnchor);

            var topLineAnchor = new BasisPoint(sideLineAnchor.Point.Left(1.75d), "#topLineAnchor");
            var topLine = new LineElement("#topLine")
            {
                StrokeColor = SColor.White,
                StrokeWidth = 4d,
                ToPoint = SPointF.Zero.Right(_charWidth!.Value).Right((LineMargin * 4d) + 1.75d)
            };
            topLine.AnchorTopLeftTo(topLineAnchor);

            var dividendAnchor = new BasisPoint(sideLineAnchor.Point.Right(StrokeWidth + (LineMargin * 4d)), "#dividendAnchor");
            var dividend = MakeTextBlock("#dividend", "1");
            dividend.AnchorTopLeftTo(dividendAnchor);

            Add([divisorAnchor, divisor, sideLineAnchor, sideLine, topLine, topLineAnchor, dividendAnchor, dividend])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowDividendAndDivisor, State.Step01)]
        private void ToStep01()
        {
            // Demonstrates that we have to treat it like 10 / 3 first because 3 > 1
            var topLineAnchor = (BasisPoint)QueryBasis("#topLineAnchor").Single();

            SPointF quotientPoint = topLineAnchor.Point
                .Up(_lineHeight!.Value + LineMargin)
                .Right(LineMargin * 4d)
                .Right(5.3d); // AAAA it's five pixels short AAAA
            var quotientAnchor = new BasisPoint(quotientPoint, "#quotientAnchor");
            var quotient = MakeTextBlock("#quotient", "0.");
            quotient.AnchorTopLeftTo(quotientAnchor);

            SetTopLineWidth(3);
            SetTextBlock("#dividend", "1.0");
            Add([quotientAnchor, quotient])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.Step01, State.Step02)]
        private void ToStep02()
        {
            // Okay, now we can start the long division process. We have 1.0 / 0.3, which gives us a quotient
            // of 0.3 and a remainder of 1.
            SetTextBlock("#quotient", "0.3");

            var dividend = (TextBlock)Query("#dividend").Single();
            var dividendRight = dividend.Bounds.CenterRight;
            var line0Anchor = new BasisPoint(dividendRight.Down(_lineHeight!.Value + LineMargin), "#line0Anchor");
            var line0 = MakeTextBlock("#line0", "- 9");
            line0.AnchorRightCenterTo(line0Anchor);

            Add([line0Anchor, line0])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.Step02, State.Step03)]
        private void ToStep03()
        {
            var divider0Anchor = GetDividerAnchor(GetCenterRightOf("#line0"), "0");
            LineElement divider0 = MakeDivider(divider0Anchor, "0");

            var line1Anchor = new BasisPoint(divider0.Bounds.CenterRight.Down((_lineHeight!.Value / 2d) + LineMargin), "#line1Anchor");
            var line1 = MakeTextBlock("#line1", "1");
            line1.AnchorRightCenterTo(line1Anchor);

            Add([divider0Anchor, divider0, line1Anchor, line1])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.Step03, State.Step04)]
        private void ToStep04()
        {
            // Now we can do more in one step. Carry another 0 down, get another digit in the quotient, and get a new remainder of 1 again.
            SetTextBlock("#quotient", "0.33");
            SetTextBlock("#dividend", "1.00");
            SetTextBlock("#line1", "10");
            MoveAnchorRight("#line1Anchor", _charWidth!.Value);
            SetTopLineWidth(4);

            var line1Point = GetCenterRightOf("#line1");
            var line2Anchor = new BasisPoint(line1Point.Down(_lineHeight!.Value + LineMargin), "#line2Anchor");
            var line2 = MakeTextBlock("#line2", "- 9");
            line2.AnchorRightCenterTo(line2Anchor);
            Add([line2Anchor, line2])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.Step04, State.Step05)]
        private void ToStep05()
        {
            var divider1Anchor = GetDividerAnchor(GetCenterRightOf("#line2"), "1");
            var divider1 = MakeDivider(divider1Anchor, "1");
            var line3Anchor = new BasisPoint(divider1.Bounds.CenterRight.Down((_lineHeight!.Value / 2d) + LineMargin), "#line3Anchor");
            var line3 = MakeTextBlock("#line3", "1");
            line3.AnchorRightCenterTo(line3Anchor);
            Add([divider1Anchor, divider1, line3Anchor, line3])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.Step05, State.FinishProblem)]
        private void ToFinishProblem()
        {
            // Just keep adding lines until our quotient overflows the right, but all at once.
            var addedElements = new List<ISlideAddable>();
            var lastLineIndex = 3;
            var lastDividerIndex = 2;
            var addedDigits = 0;

            string? quotientText = ((TextBlock)Query("#quotient").Single()).Text;
            string? dividendText = ((TextBlock)Query("#dividend").Single()).Text;

            // We use the f_ prefix to indicate that these should all be removed as one when rewinding.
            while (GetCenterRightOf("#quotient").X < Size.Width)
            {
                quotientText += "3";
                dividendText += "0";
                SetTextBlock("#quotient", quotientText);
                SetTextBlock("#dividend", dividendText);
                addedDigits += 1;

                // Expand the "1" to a "10" and move the anchor right
                var lineSelector = $"#line{lastLineIndex}";
                var lineAnchorSelector = $"#line{lastLineIndex}Anchor";
                SetTextBlockInternal(lineSelector, "10");
                MoveAnchorRightInternal(lineAnchorSelector, _charWidth!.Value);

                // Add a new line with "- 9"
                var linePoint = GetCenterRightOfInternal(lineSelector);
                lastLineIndex += 1;
                var newLine = MakeTextBlock($"#f_line{lastLineIndex}", "- 9");
                var newLineAnchor = new BasisPoint(linePoint.Down(_lineHeight!.Value + LineMargin), $"#f_line{lastLineIndex}Anchor");
                newLine.AnchorRightCenterTo(newLineAnchor);
                addedElements.Add(newLineAnchor);
                addedElements.Add(newLine);

                // Add a divider
                var dividerAnchor = GetDividerAnchor(GetCenterRightOfInternal($"#f_line{lastLineIndex}"), lastDividerIndex.ToString());
                var divider = MakeDividerInternal(dividerAnchor, lastDividerIndex.ToString());
                addedElements.Add(dividerAnchor);
                addedElements.Add(divider);
                lastDividerIndex += 1;

                // Add a new line with "1"
                lastLineIndex += 1;
                var linePoint2 = GetCenterRightOfInternal($"#f_line{lastLineIndex - 1}");
                var newLine2 = MakeTextBlock($"#f_line{lastLineIndex}", "1");
                var newLineAnchor2 = new BasisPoint(linePoint2.Down(_lineHeight!.Value + LineMargin), $"#f_line{lastLineIndex}Anchor");
                newLine2.AnchorRightCenterTo(newLineAnchor2);
                addedElements.Add(newLineAnchor2);
                addedElements.Add(newLine2);
            }

            Add(addedElements)
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
            SetTopLineWidth(quotientText.Length);

            SPointF GetCenterRightOfInternal(string selector)
            {
                var element = Query(selector).SingleOrDefault() ?? addedElements.Single(e =>
                {
                    // This is a bit sad, but we have to check both the main elements and the ones we just
                    // added because of how the state machine adds elements before animating them.
                    var selectorFixed = selector.Replace("#", "");
                    if (!selectorFixed.StartsWith("f_"))
                    {
                        selectorFixed = "f_" + selectorFixed;
                    }
                    if (e is AtriaElement atriaElement)
                    {
                        return atriaElement.Id.Id == selectorFixed;
                    }
                    else if (e is BasisPoint basisPoint)
                    {
                        return basisPoint.Id.Id == selectorFixed;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown element type in GetCenterRightOfInternal");
                    }
                });
                return element is AtriaElement ae
                    ? ae.Bounds.CenterRight
                    : element is BasisPoint bp
                        ? bp.Point
                        : throw new InvalidOperationException("Unknown element type in GetCenterRightOfInternal");
            }

            void SetTextBlockInternal(string selector, string newText)
            {
                var textBlock = (TextBlock)Query(selector).SingleOrDefault() ?? addedElements.OfType<TextBlock>().Single(e => e.Id.Id == "f_" + selector.Replace("#", ""));
                textBlock.SetTextAndKeepFontSize(MeasurementService, newText);
            }

            void MoveAnchorRightInternal(string selector, double distance)
            {
                var anchor = (BasisPoint)QueryBasis(selector).SingleOrDefault() ?? addedElements.OfType<BasisPoint>().Single(e => e.Id.Id == "f_" + selector.Replace("#", ""));
                anchor.Point = anchor.Point.Right(distance);
            }

            LineElement MakeDividerInternal(BasisPoint anchor, string dividerIndex)
            {
                var divider0 = new LineElement($"#f_divider{dividerIndex}")
                {
                    StrokeColor = SColor.White,
                    StrokeWidth = StrokeWidth,
                    ToPoint = SPointF.Zero.Right(_charWidth!.Value * 3d)
                };
                divider0.AnchorLeftCenterTo(anchor);
                return divider0;
            }
        }

        // Reverse transitions
        [StateTransition<State>(State.FinishProblem, State.Step05)]
        private void BackToStep05()
        {
            // Just remove all the elements we added in the last step.
            var elementsToRemove = Elements.Where(e => e.Id.Id!.StartsWith("f_"))
                .Cast<ISlideAddable>()
                .Concat(BasisElements.Where(e => e.Id.Id!.StartsWith("f_")).Cast<ISlideAddable>())
                .ToArray(); // make array to avoid modifying the collection while enumerating
            Remove(elementsToRemove);
            // Remove the digits we added to the quotient and dividend, and move the line anchor back.
            SetTextBlock("#quotient", "0.33");
            SetTextBlock("#dividend", "1.00");
            var topLine = (LineElement)Query("#topLine").Single();
            topLine.ToPoint = SPointF.Zero.Right(_charWidth!.Value * 4).Right((LineMargin * 4d) + 1.75d);

            // Move #line3 back to the left and change it back to "1"
            SetTextBlock("#line3", "1");
            MoveAnchorRight("#line3Anchor", -_charWidth!.Value);
        }

        [StateTransition<State>(State.Step05, State.Step04)]
        private void BackToStep04()
        {
            Remove([(ISlideAddable)QueryBasis("#divider1Anchor").Single(), Query("#divider1").Single(), (ISlideAddable)QueryBasis("#line3Anchor").Single(), Query("#line3").Single()]);
            SetTextBlock("#quotient", "0.3");
            SetTextBlock("#dividend", "1.00");
            SetTextBlock("#line1", "10");
            SetTopLineWidth(4);
        }

        [StateTransition<State>(State.Step04, State.Step03)]
        private void BackToStep03()
        {
            Remove([(ISlideAddable)QueryBasis("#line2Anchor").Single(), Query("#line2").Single()]);
            SetTextBlock("#quotient", "0.3");
            SetTextBlock("#dividend", "1.0");
            SetTextBlock("#line1", "1");
            MoveAnchorRight("#line1Anchor", -_charWidth!.Value);
            SetTopLineWidth(3);
        }

        [StateTransition<State>(State.Step03, State.Step02)]
        private void BackToStep02()
        {
            Remove([(ISlideAddable)QueryBasis("#divider0Anchor").Single(), Query("#divider0").Single(),
                (ISlideAddable)QueryBasis("#line0Anchor").Single(), Query("#line0").Single()]);
            SetTextBlock("#quotient", "0.");
            SetTopLineWidth(2);
        }

        [StateTransition<State>(State.Step02, State.Step01)]
        private void BackToStep01()
        {
            Remove([(ISlideAddable)QueryBasis("#quotientAnchor").Single(), Query("#quotient").Single()]);
            SetTopLineWidth(1);
            SetTextBlock("#dividend", "1");
        }

        [StateTransition<State>(State.Step01, State.ShowDividendAndDivisor)]
        private void BackToShowDividendAndDivisor()
        {
            Remove([(ISlideAddable)QueryBasis("#quotientAnchor").Single(), Query("#quotient").Single()]);
            SetTopLineWidth(1);
            SetTextBlock("#dividend", "1");
        }

        [StateTransition<State>(State.ShowDividendAndDivisor, State.Initial)]
        private void BackToInitial()
        {
            Remove([(ISlideAddable)QueryBasis("#divisorAnchor").Single(), Query("#divisor").Single(),
                (ISlideAddable)QueryBasis("#sideLineAnchor").Single(), Query("#sideLine").Single(),
                (ISlideAddable)QueryBasis("#topLineAnchor").Single(), Query("#topLine").Single(),
                (ISlideAddable)QueryBasis("#dividendAnchor").Single(), Query("#dividend").Single()]);
        }

        private TextBlock MakeTextBlock(string id, string text)
        {
            return new TextBlock(id)
            {
                Text = text,
                FontFamily = FontFamily,
                FontSize = FontSize,
                Color = SColor.White
            };
        }

        private void SetTextBlock(string selector, string newText)
        {
            var textBlock = (TextBlock)Query(selector).Single();
            textBlock.SetTextAndKeepFontSize(MeasurementService, newText);
        }

        private void MoveAnchorRight(string selector, double distance)
        {
            var anchor = (BasisPoint)QueryBasis(selector).Single();
            anchor.Point = anchor.Point.Right(distance);
        }

        private SPointF GetCenterRightOf(string selector)
        {
            var element = Query(selector).Single();
            return element.Bounds.CenterRight;
        }

        private void SetTopLineWidth(int characterWidth)
        {
            var topLine = (LineElement)Query("#topLine").Single();
            var width = (characterWidth * _charWidth!.Value) + (LineMargin * 4d) + 1.75d;
            topLine!.ToPoint = SPointF.Zero.Right(width);
        }

        private BasisPoint GetDividerAnchor(SPointF line0Right, string dividerIndex)
        {
            return new BasisPoint(line0Right.Down((_lineHeight!.Value / 2d) + LineMargin).Left(_charWidth!.Value * 3d), $"#divider{dividerIndex}Anchor");
        }

        private LineElement MakeDivider(BasisPoint anchor, string dividerIndex)
        {
            var divider0 = new LineElement($"#divider{dividerIndex}")
            {
                StrokeColor = SColor.White,
                StrokeWidth = StrokeWidth,
                ToPoint = SPointF.Zero.Right(_charWidth!.Value * 3d)
            };
            divider0.AnchorLeftCenterTo(anchor);
            return divider0;
        }
    }
}
