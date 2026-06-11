using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests.Operations
{
    internal sealed class ShortOperationSlide : AtriaSlide
    {
        private const int _startAndEndHold = 60;
        private const double _yStepMultiplier = 1.01d;

        private Func<short, short, short> _transform;
        private int _frameNumber;
        private readonly string _operationText;
        private readonly ShortGrid _shortGridElement;
        private readonly TextBlock _yTextElement;
        private int? inEndHoldFrame;

        public bool IsUnary => !_operationText.Contains('y', StringComparison.InvariantCultureIgnoreCase);

        public bool Completed
        {
            get; private set;
        }

        public ShortOperationSlide(Func<short, short, short> transform,
            string operationText,
            int width,
            int height,
            double sizeMultiplier = 1d) : base(width, height)
        {
            _transform = transform;
            _operationText = operationText;
            BackgroundColor = new SColor(8, 0, 130, 255);
            var shortGridElement = new ShortGrid(b => transform(b, 0))
            {
                Size = new SSizeF(500 * (float)sizeMultiplier, 500 * (float)sizeMultiplier)
            };
            var leftRightMargin = 0.6f;
            var hCenterBasisLeft = new BasisLine(LeftCenter, RightCenter)
                .SplitAndTakeLeft(leftRightMargin);
            var gridAnchor = new BasisPoint(hCenterBasisLeft.Center, "#gridAnchor");
            shortGridElement.AnchorCenterTo(gridAnchor);
            Add([shortGridElement, gridAnchor]);
            _shortGridElement = shortGridElement;

            var textHeight = 75f * (float)sizeMultiplier;
            var operationTextElement = new TextBlock("#operation")
            {
                Text = operationText,
                Size = new SSizeF(200f * (float)sizeMultiplier, textHeight),
                Color = SColor.White,
                FontFamily = "Consolas",
                FontSize = 36f * (float)sizeMultiplier
            };
            var yTextElement = new TextBlock("#yText")
            {
                Text = "",
                Size = new SSizeF(200f * (float)sizeMultiplier, textHeight),
                Color = SColor.White,
                FontFamily = "Consolas",
                FontSize = 36f * (float)sizeMultiplier
            };
            _yTextElement = yTextElement;

            var hCenterBasisRight = new BasisLine(LeftCenter, RightCenter)
                .SplitAndTakeRight(1 - leftRightMargin);
            var operationTextAnchor = new BasisPoint(hCenterBasisRight.Center
                .Up(textHeight / 2), "#operationTextAnchor");
            var yTextAnchor = new BasisPoint(hCenterBasisRight.Center
                .Down(textHeight / 2), "#yTextAnchor");
            operationTextElement.AnchorCenterTo(operationTextAnchor);
            yTextElement.AnchorCenterTo(yTextAnchor);
            Add([operationTextElement, yTextElement, operationTextAnchor, yTextAnchor]);
        }

        public override void Update(double deltaTime)
        {
            _frameNumber += 1;

            // Wait 1 second before animating
            if (_frameNumber < _startAndEndHold) { return; }

            // Check if we're in the end hold
            if (inEndHoldFrame.HasValue)
            {
                // If we've been in the end hold for long enough, mark as completed
                if (_frameNumber - inEndHoldFrame.Value >= _startAndEndHold)
                {
                    Completed = true;
                }
                return;
            }

            // Compute y = (_yStepMultiplier ^ frameNumber)
            var adjustedFrameNumber = _frameNumber - _startAndEndHold;
            var y = (int)Math.Pow(_yStepMultiplier, adjustedFrameNumber);
            if (y > 65535)
            {
                y = 65535;
                inEndHoldFrame = _frameNumber;
            }

            _shortGridElement.Transform = b => _transform(b, (short)y);

            if (_operationText.Contains('y', StringComparison.InvariantCultureIgnoreCase))
            {
                _yTextElement.Text = $"y = 0x{(short)y:X4}";
            }
        }

        public override void Initialize()
        {
            
        }
    }
}
