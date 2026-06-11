using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    public sealed class CountPanelElement : AtriaElement
    {
        private static readonly SFont _font = new SFontFamily("Consolas", 48f);
        private static readonly SColor _edgeColor = SColor.White;

        private static readonly string[] _tierTexts = ["Ones", "Thousands", "Millions", "Billions", "Trillions",
            "Quadrillions", "Quintillions", "Sextillions", "Septillions", "Octillions", "Nonillions", "Decillions",
            "Undecillions", "Duodecillions", "Tredecillions", "Quattuordecillions", "Quindecillions", "Sexdecillions",
            "Septendecillions", "Octodecillions", "Novemdecillions", "Vigintillions", "Unvigintillions"];
        private static readonly SColor[] _tierBackgroundColors = [
            SColor.FromHSV(0d, 0d, 0.4d),
            SColor.FromHSV(0d, 0d, 0.4d * 1.8d),
            SColor.FromHSV(0d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(30d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(60d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(120d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(160d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(240d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(270d, 0.8d, 0.4d * 1.8d),
            SColor.FromHSV(0d, 1d, 0.7d),
            SColor.FromHSV(30d, 1d, 0.7d),
            SColor.FromHSV(60d, 1d, 0.7d),
            SColor.FromHSV(120d, 1d, 0.7d),
            SColor.FromHSV(240d, 1d, 0.7d),
            SColor.FromHSV(270d, 1d, 0.7d),
            SColor.FromHSV(0d, 1d, 0.8d),
            SColor.FromHSV(30d, 1d, 0.8d),
            SColor.FromHSV(60d, 1d, 0.8d),
            SColor.FromHSV(120d, 1d, 0.8d),
            SColor.FromHSV(240d, 1d, 0.8d),
            SColor.FromHSV(270d, 1d, 0.8d),
        ];
        private static readonly SColor[] _tierDividerColors = [
            SColor.FromHSV(0d, 0d, 0.2d),
            SColor.FromHSV(0d, 0d, 0.2d * 1.8d),
            SColor.FromHSV(0d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(30d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(60d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(120d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(160d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(240d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(270d, 0.8d, 0.2d * 1.8d),
            SColor.FromHSV(0d, 1d, 0.5d),
            SColor.FromHSV(30d, 1d, 0.5d),
            SColor.FromHSV(60d, 1d, 0.5d),
            SColor.FromHSV(120d, 1d, 0.5d),
            SColor.FromHSV(240d, 1d, 0.5d),
            SColor.FromHSV(270d, 1d, 0.5d),
            SColor.FromHSV(0d, 1d, 1d),
            SColor.FromHSV(30d, 1d, 1d),
            SColor.FromHSV(60d, 1d, 1d),
            SColor.FromHSV(120d, 1d, 1d),
            SColor.FromHSV(240d, 1d, 1d),
            SColor.FromHSV(270d, 1d, 1d),
        ];
        private static readonly SColor[] _tierUnfilledBoxColors = [
            SColor.FromHSV(0d, 0d, 0.333d),
            SColor.FromHSV(0d, 0d, 0.333d * 1.8d),
            SColor.FromHSV(0d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(30d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(60d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(120d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(160d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(240d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(270d, 0.8d, 0.333d * 1.8d),
            SColor.FromHSV(0d, 1d, 0.8d),
            SColor.FromHSV(30d, 1d, 0.8d),
            SColor.FromHSV(60d, 1d, 0.8d),
            SColor.FromHSV(120d, 1d, 0.8d),
            SColor.FromHSV(240d, 1d, 0.8d),
            SColor.FromHSV(270d, 1d, 0.8d),
            SColor.FromHSV(0d, 1d, 0.7d),
            SColor.FromHSV(30d, 1d, 0.7d),
            SColor.FromHSV(60d, 1d, 0.7d),
            SColor.FromHSV(120d, 1d, 0.7d),
            SColor.FromHSV(240d, 1d, 0.7d),
            SColor.FromHSV(270d, 1d, 0.7d),
        ];
        private static readonly SColor[] _tierFilledBoxColors = [
            SColor.FromHSV(0d, 0d, 0.533d),
            SColor.FromHSV(0d, 0d, 0.533d * 1.8d),
            SColor.FromHSV(0d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(30d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(60d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(120d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(160d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(240d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(270d, 0.8d, 0.533d * 1.8d),
            SColor.FromHSV(0d, 1d, 1d),
            SColor.FromHSV(30d, 1d, 1d),
            SColor.FromHSV(60d, 1d, 1d),
            SColor.FromHSV(120d, 1d, 1d),
            SColor.FromHSV(160d, 1d, 1d),
            SColor.FromHSV(240d, 1d, 1d),
            SColor.FromHSV(270d, 1d, 1d),
            SColor.FromHSV(0d, 1d, 0.5d),
            SColor.FromHSV(30d, 1d, 0.5d),
            SColor.FromHSV(60d, 1d, 0.5d),
            SColor.FromHSV(120d, 1d, 0.5d),
            SColor.FromHSV(240d, 1d, 0.5d),
            SColor.FromHSV(270d, 1d, 0.5d),
        ];

        private BigInteger _count;
        private SSizeF _countBounds;

        public BigInteger Count
        {
            get => _count;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Count), "Count cannot be negative.");
                _count = value;
                // TODO: We need to figure out how to handle the case where Slide is null here.
                _countBounds = Slide?.MeasurementService.MeasureText(CountText, _font) ?? SSizeF.Zero;
            }
        }

        private int Tier
        {
            get
            {
                var log10 = BigInteger.Log10(_count);
                if (log10 < 0) { log10 = 0; }
                return ((int)Math.Floor(log10)) / 3;
            }
        }

        private string TierText
        {
            get
            {
                if (Tier >= 22) { return "Out of range"; }
                return _tierTexts[Tier];
            }
        }

        private string SmallTierText
        {
            get
            {
                var exponent = Tier * 3;
                var exponentText = new string(exponent.ToString().Select(ToUnicodeSuperscript).ToArray());
                return $"10{exponentText}";
            }
        }

        /// <summary>
        /// Gets the full text of the count, formatted with spaces between every three digits
        /// (i.e. 1 234 567).
        /// </summary>
        private string CountText => _count.ToString("N0").Replace(",", " ");

        private SColor BackgroundColor => _tierBackgroundColors[Tier % _tierBackgroundColors.Length];
        private SColor DividerColor => _tierDividerColors[Tier % _tierDividerColors.Length];
        private SColor UnfilledBoxColor => _tierUnfilledBoxColors[Tier % _tierUnfilledBoxColors.Length];
        private SColor FilledBoxColor => _tierFilledBoxColors[Tier % _tierFilledBoxColors.Length];

        private int FilledBoxCount
        {
            get
            {
                var tier = Tier;
                var count = Count;
                while (tier > 0)
                {
                    count /= 1000;
                    tier--;
                }
                return (int)count;
            }
        }

        private static readonly BigInteger PlanckTimesPerSecond = BigInteger.Parse("1854858439986147917170183447354600000000000");
        private double _elapsedSeconds;
        public override void Update(double deltaTime)
        {
            _elapsedSeconds += deltaTime;
            var elapsed60HzFrames = _elapsedSeconds * 60d;
            var elapsedNtscFields = _elapsedSeconds * 59.94d;
            var difference = elapsed60HzFrames - elapsedNtscFields;
            Count = (BigInteger)difference;
        }

        public override void Render(IRenderTarget target)
        {
            var measurementService = Slide?.MeasurementService;
            if (measurementService == null) { return; }

            if (_countBounds == SSizeF.Zero)
            {
                _countBounds = measurementService.MeasureText(CountText, _font);
            }

            target.DrawRectangle(Bounds, BackgroundColor, SPaintStyle.Fill, SAngle.Zero);
            target.DrawRectangle(Bounds, _edgeColor, SPaintStyle.Stroke, SAngle.Zero);

            var centerPanelHeightRatio = 7d;
            var totalSegments = 1 + 1 + centerPanelHeightRatio;
            var topAndBottomHeight = Size.Height / totalSegments;
            var centerPanelHeight = (Size.Height * centerPanelHeightRatio) / totalSegments;
            var dividerLineHeight = 2d;
            topAndBottomHeight -= dividerLineHeight / 2d;
            centerPanelHeight -= dividerLineHeight;

            var topY = Bounds.Top;
            var firstDividerY = topY + topAndBottomHeight;
            var centerPanelY = firstDividerY + dividerLineHeight;
            var secondDividerY = centerPanelY + centerPanelHeight;
            var bottomY = secondDividerY + dividerLineHeight;

            var dividerLineWidthRatio = 0.95d;
            var dividerLineWidth = Size.Width * dividerLineWidthRatio;
            var dividerHorizontalMargin = (1 - dividerLineWidthRatio) / 2d;
            var dividerLeft = Bounds.Left + (Size.Width * dividerHorizontalMargin);
            var dividerRight = Bounds.Right - (Size.Width * dividerHorizontalMargin);

            var dividerLineCenterOffset = dividerLineHeight / 2d;
            target.DrawLine(new SPointF(dividerLeft, firstDividerY), new SPointF(dividerRight, firstDividerY), DividerColor, (float)dividerLineHeight);
            target.DrawLine(new SPointF(dividerLeft, secondDividerY), new SPointF(dividerRight, secondDividerY), DividerColor, (float)dividerLineHeight);

            var textHMarginRatio = dividerHorizontalMargin;
            var textLeftX = Bounds.Left + (Size.Width * textHMarginRatio);
            var textRightX = Bounds.Right - (Size.Width * textHMarginRatio);
            var textVMarginOfSegmentRatio = 0.95d;
            var textHeight = topAndBottomHeight * textVMarginOfSegmentRatio;
            var textVMargin = (topAndBottomHeight - textHeight) / 2d;

            var tierTextAspectRatio = measurementService.AspectRatioForText(TierText, _font);
            var tierTextWidth = textHeight * tierTextAspectRatio;
            var tierTextPosition = new SPointF(textLeftX, topY + textVMargin);
            SRectF tierTextBounds = tierTextPosition.WithSize(new(tierTextWidth, textHeight));
            target.DrawText(TierText, _font, tierTextBounds, SColor.White, SAngle.Zero, Alignment.LeftCenter);

            var smallTierTextAspectRatio = measurementService.AspectRatioForText(SmallTierText, _font);
            var smallTierTextWidth = textHeight * smallTierTextAspectRatio;
            var smallTierTextLeftX = textRightX - smallTierTextWidth;
            var smallTierTextBounds = new SRectF(smallTierTextLeftX, tierTextPosition.Y, smallTierTextWidth, textHeight);
            target.DrawText(SmallTierText, _font, smallTierTextBounds, SColor.White, SAngle.Zero, Alignment.RightCenter);

            var countTextAspectRatio = measurementService.AspectRatioForText(CountText, _font);
            var countTextWidth = textHeight * countTextAspectRatio;
            var countTextHeight = textHeight;
            var countTextMaxWidth = textRightX -  textLeftX;
            if (countTextWidth > countTextMaxWidth)
            {
                countTextWidth = countTextMaxWidth;
                countTextHeight = countTextWidth / countTextAspectRatio;
            }
            var countTextBounds = new SRectF(textRightX - countTextWidth,
                bottomY + textVMargin, countTextWidth, countTextHeight);
            target.DrawText(CountText, _font, countTextBounds, SColor.White, SAngle.Zero, Alignment.RightCenter);

            // OKAY. Now we can measure and draw the boxes.
            var innerMarginRatio = 0.05d;
            var centerPanelOuterSize = new SSizeF(dividerLineWidth, secondDividerY - centerPanelY);
            var centerPanelInnerSize = centerPanelOuterSize.ShrinkTowardCenter(centerPanelOuterSize.Width * innerMarginRatio, centerPanelOuterSize.Height * innerMarginRatio);
            var centerPanelCorrectRatio = centerPanelInnerSize.FitAspectRatioInside(2d / 1d);
            var centerPanelOuterBounds = centerPanelOuterSize.At(new SPointF(Bounds.Left + dividerHorizontalMargin, centerPanelY)).RoundStandard();
            var centerPanelCorrectBounds = centerPanelCorrectRatio.CenterAt(centerPanelOuterBounds.Center).RoundStandard();

            var boxToMarginRatio = 3d;
            var boxSegmentsH = 50;
            var marginSegmentsH = boxSegmentsH - 1;
            var availableWidth = centerPanelCorrectBounds.Width;
            // There are 49 (box + margin) segments then 1 last box segment, so 50 total box widths and 49 total margin widths.
            var marginWidth = 1d;
            var boxWidth = boxToMarginRatio * marginWidth;
            var boxAndMarginWidth = boxWidth + marginWidth;
            var totalBoxAndMarginWidth = (boxAndMarginWidth * marginSegmentsH) + boxWidth;
            var marginPixelWidth = availableWidth / totalBoxAndMarginWidth;
            var boxPixelWidth = marginPixelWidth * boxToMarginRatio;

            var boxSegmentsV = 20;
            var marginSegmentsV = boxSegmentsV - 1;
            var availableHeight = centerPanelCorrectBounds.Height;
            var marginHeight = 1d;
            var boxHeight = boxToMarginRatio * marginHeight;
            var boxAndMarginHeight = boxHeight + marginHeight;
            var totalBoxAndMarginHeight = (boxAndMarginHeight * marginSegmentsV) + boxHeight;
            var marginPixelHeight = availableHeight / totalBoxAndMarginHeight;
            var boxPixelHeight = marginPixelHeight * boxToMarginRatio;

            var filledBoxCount = FilledBoxCount;
            
            for (int j = 0; j < boxSegmentsV; j++)
            {
                var boxTop = centerPanelCorrectBounds.Top + (j * (boxPixelHeight + marginPixelHeight));
                for (int i = 0; i < boxSegmentsH; i++)
                {
                    var boxIndex = (j * boxSegmentsH) + i;
                    var boxLeft = centerPanelCorrectBounds.Left + (i * (boxPixelWidth + marginPixelWidth));
                    var boxBounds = new SRectF(boxLeft, boxTop, boxPixelWidth, boxPixelHeight);
                    target.DrawRectangle(boxBounds, boxIndex < filledBoxCount ? FilledBoxColor : UnfilledBoxColor, SPaintStyle.Fill, SAngle.Zero);
                }
            }
        }

        private static char ToUnicodeSuperscript(char digit)
        {
            return digit switch
            {
                '0' => '⁰',
                '1' => '¹',
                '2' => '²',
                '3' => '³',
                '4' => '⁴',
                '5' => '⁵',
                '6' => '⁶',
                '7' => '⁷',
                '8' => '⁸',
                '9' => '⁹',
                '-' => '⁻',
                _ => throw new ArgumentException($"Invalid digit character: {digit}")
            };
        }
    }
}
